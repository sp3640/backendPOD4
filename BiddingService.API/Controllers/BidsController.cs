using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.BiddingService.Data;
using OnlineAuctionSystem.BiddingService.DTOs;
using OnlineAuctionSystem.BiddingService.Models;
using System.Security.Claims;
using System.Text.Json;

namespace OnlineAuctionSystem.BiddingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BidsController : ControllerBase
    {
        private readonly BiddingDbContext _context;
        private readonly HttpClient _httpClient; // Used for calling Auction Service

        // Auction DTO used internally for communication
        private class AuctionDetailsDto
        {
            public decimal HighestBid { get; set; }
            public string? HighestBidderUsername { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        public BidsController(BiddingDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            // Base address should be configured to the Auction Service URL
            // E.g., http://localhost:5002/
            httpClient.BaseAddress = new Uri(configuration["AuctionService:Url"]
                                             ?? throw new ArgumentNullException("AuctionService:Url not configured."));
            _httpClient = httpClient;
        }

        // GET: api/Bids/{auctionId} (Public access - view bid history)
        [HttpGet("{auctionId:guid}")]
        public async Task<ActionResult<IEnumerable<Bid>>> GetBidsByAuction(Guid auctionId)
        {
            return await _context.Bids
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.Timestamp)
                .ToListAsync();
        }

        // POST: api/Bids (Secured: Buyer Role Required)
        [Authorize(Roles = "Buyer")]
        [HttpPost]
        public async Task<IActionResult> PlaceBid([FromBody] BidPlaceDto dto)
        {
            var bidderUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(bidderUsername)) return Unauthorized();

            // 1. Fetch current auction details from Auction Service
            AuctionDetailsDto? auction;
            try
            {
                var response = await _httpClient.GetAsync($"api/Auctions/{dto.AuctionId}");
                Console.WriteLine(dto.AuctionId);
                if (!response.IsSuccessStatusCode) return BadRequest("Auction not found or inaccessible.");

                var content = await response.Content.ReadAsStringAsync();
                auction = JsonSerializer.Deserialize<AuctionDetailsDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception)
            {
                // Handle network or serialization errors
                return StatusCode(503, "Cannot communicate with Auction Service."+ dto.AuctionId);
            }

            // 2. Validate Bid
            if (auction?.Status != "Live") return BadRequest("Auction is not live.");
            if (dto.Amount <= auction.HighestBid)
                return BadRequest($"Bid must be greater than the current highest bid: ${auction.HighestBid:F2}");
            if (bidderUsername == auction.HighestBidderUsername)
                return BadRequest("You are already the highest bidder.");


            // 3. Save the new Bid
            var newBid = new Bid
            {
                AuctionId = dto.AuctionId,
                BidderUsername = bidderUsername,
                Amount = dto.Amount,
                Timestamp = DateTime.UtcNow
            };
            _context.Bids.Add(newBid);
            await _context.SaveChangesAsync();

            // 4. Communicate back to Auction Service to update its HighestBid fields
            // The Bidding Service uses its own JWT token to call the Auction Service's internal endpoint
            try
            {
                // Note: The Auction Service endpoint is PUT api/Auctions/highestBid/{auctionId}?amount=...&bidderUsername=...
                var token = GetInternalServiceToken(); // Placeholder for internal token generation/retrieval
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var updateResponse = await _httpClient.PutAsync(
                    $"api/Auctions/highestBid/{dto.AuctionId}?amount={dto.Amount}&bidderUsername={bidderUsername}",
                    null // No body needed for query parameters
                );

                if (!updateResponse.IsSuccessStatusCode)
                {
                    // Log an error here: consistency issue
                    // We saved the bid, but failed to update the Auction Service
                }
            }
            catch (Exception ex)
            {
                // Log or handle communication error with Auction Service
            }

            return CreatedAtAction(nameof(GetBidsByAuction), new { auctionId = newBid.AuctionId }, newBid);
        }

        // Placeholder: In a real environment, you'd use a service principal or 
        // client credentials flow to get a token recognized by the Auction Service
        private string GetInternalServiceToken()
        {
            // For a simple demo, you might generate a short-lived token here or 
            // use the token passed by the frontend if you trust the Auction Service 
            // to re-validate it, but a dedicated service-to-service token is best.
            // Since the Auction Service's UpdateHighestBid endpoint just requires [Authorize], 
            // we assume a simple, dedicated internal key is used for this microservice communication.
            return User.FindFirstValue("InternalServiceToken") ?? "PlaceholderToken";
        }
    }
}