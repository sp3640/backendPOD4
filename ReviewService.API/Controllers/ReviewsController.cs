using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.ReviewService.Data;
using OnlineAuctionSystem.ReviewService.DTOs;
using OnlineAuctionSystem.ReviewService.Models;
using System.Security.Claims;
using System.Text.Json;

namespace OnlineAuctionSystem.ReviewService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly ReviewDbContext _context;
        private readonly HttpClient _httpClient; // Used for calling Payment Service

        public ReviewsController(ReviewDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            // Base address should be configured to the Payment Service URL
            httpClient.BaseAddress = new Uri(configuration["PaymentService:Url"]
                                             ?? throw new ArgumentNullException("PaymentService:Url not configured."));
            _httpClient = httpClient;
        }

        // GET: api/Reviews/{username} (Public access - view all reviews for a user)
        [HttpGet("{username}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews(string username)
        {
            return await _context.Reviews
                .Where(r => r.ReviewedUsername == username)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
        }

        // GET: api/Reviews/rating/{username} (Public access - get average rating)
        [HttpGet("rating/{username}")]
        public async Task<IActionResult> GetAverageRating(string username)
        {
            var ratings = await _context.Reviews
                .Where(r => r.ReviewedUsername == username)
                .Select(r => r.Rating)
                .ToListAsync();

            if (!ratings.Any())
            {
                return Ok(new { username, averageRating = 0.0, count = 0 });
            }

            return Ok(new
            {
                username,
                averageRating = ratings.Average(),
                count = ratings.Count
            });
        }

        // --- THIS IS THE NEW ENDPOINT YOU REQUESTED ---
        // GET: api/Reviews/auction/{auctionId:guid}
        [HttpGet("auction/{auctionId:guid}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsForAuction(Guid auctionId)
        {
            return await _context.Reviews
                .Where(r => r.AuctionId == auctionId)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
        }
        // --- END OF NEW ENDPOINT ---

        // POST: api/Reviews (Secured: Any authenticated user)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostReview([FromBody] ReviewCreateDto dto)
        {
            var reviewerUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(reviewerUsername)) return Unauthorized();

            // 1. Validation: Ensure a completed transaction exists for this AuctionId
            bool transactionExists = false;
            try
            {
                // We call the Payment Service's internal check endpoint (api/Payments/check/{auctionId})
                var response = await _httpClient.GetAsync($"api/Payments/check/{dto.AuctionId}");
                transactionExists = response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return StatusCode(503, "Cannot communicate with Payment Service for transaction validation.");
            }

            if (!transactionExists)
                return BadRequest("Review cannot be posted: No completed transaction found for this auction.");

            // 2. Prevent duplicate reviews (handled by unique index, but double-check here)
            if (await _context.Reviews.AnyAsync(r => r.AuctionId == dto.AuctionId && r.ReviewerUsername == reviewerUsername))
                return Conflict("You have already submitted a review for this transaction.");

            // 3. Save the new Review
            var review = new Review
            {
                AuctionId = dto.AuctionId,
                ReviewerUsername = reviewerUsername,
                ReviewedUsername = dto.ReviewedUsername,
                ReviewType = dto.ReviewType,
                Rating = dto.Rating,
                Comment = dto.Comment,
                Timestamp = DateTime.UtcNow
            };
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReviews), new { username = review.ReviewedUsername }, review);
        }
    }
}