using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.AuctionService.Data;
using OnlineAuctionSystem.AuctionService.DTOs;
using OnlineAuctionSystem.AuctionService.Models;
using System.Security.Claims;

namespace OnlineAuctionSystem.AuctionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;

        public AuctionsController(AuctionDbContext context)
        {
            _context = context;
        }

        // GET: api/Auctions (Public access)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Auction>>> GetAuctions()
        {
            // Simple query to fetch all auctions
            return await _context.Auctions.ToListAsync();
        }

        // GET: api/Auctions/{id} (Public access)
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Auction>> GetAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null)
            {
                return NotFound();
            }
            return auction;
        }

        // POST: api/Auctions (Secured: Seller Role Required)
        [Authorize(Roles = "Seller")]
        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromBody] AuctionCreateDto dto)
        {
            // 1. Get the authenticated Seller's username from the JWT token
            var sellerUsername = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(sellerUsername)) return Unauthorized();

            // 2. Map DTO to Model and calculate EndTime
           // var newAuction = new Auction
           var newAuction = new Auction
{
            ProductName = dto.ProductName,
             Description = dto.Description,
              SellerUsername = sellerUsername, // This was correct
                 StartPrice = dto.StartPrice,
                 StartTime = DateTime.UtcNow,
              EndTime = DateTime.UtcNow.AddMinutes(dto.DurationMinutes),
             Status = "Live",
                 ImageUrl = dto.ImageUrl // ADD THIS LINE
};

            // 3. Save to database
            _context.Auctions.Add(newAuction);
            await _context.SaveChangesAsync();

            // In a real microservice, we would publish an "AuctionCreated" event here.

            return CreatedAtAction(nameof(GetAuction), new { id = newAuction.Id }, newAuction);
        }

        // PUT: api/Auctions/highestBid (Internal/Secured endpoint for Bidding Service)
        // This endpoint would typically be called by the Bidding Service 
        // to update the highest bid locally.
        [Authorize] // Requires authentication, often using a dedicated internal role/policy
        [HttpPut("highestBid/{auctionId:guid}")]
        public async Task<IActionResult> UpdateHighestBid(Guid auctionId, [FromQuery] decimal amount, [FromQuery] string bidderUsername)
        {
            var auction = await _context.Auctions.FindAsync(auctionId);
            if (auction == null) return NotFound();

            if (amount > auction.HighestBid)
            {
                auction.HighestBid = amount;
                auction.HighestBidderUsername = bidderUsername;

                _context.Auctions.Update(auction);
                await _context.SaveChangesAsync();
                return NoContent(); // Success, nothing to return
            }

            return BadRequest("New bid is not higher than the current highest bid.");
        }
    }
}