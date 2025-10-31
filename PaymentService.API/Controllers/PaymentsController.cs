using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.PaymentService.Data;
using OnlineAuctionSystem.PaymentService.DTOs;
using OnlineAuctionSystem.PaymentService.Models;
using System.Security.Claims;
using System.Text.Json;

namespace OnlineAuctionSystem.PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentDbContext _context;
        private readonly HttpClient _httpClient;

        // Internal DTO to receive auction details from Auction Service
        private class AuctionDetailsDto
        {
            public decimal HighestBid { get; set; }
            public string? HighestBidderUsername { get; set; }
            public string? SellerUsername { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        public PaymentsController(PaymentDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            httpClient.BaseAddress = new Uri(configuration["AuctionService:Url"]
                                             ?? throw new ArgumentNullException("AuctionService:Url not configured."));
            _httpClient = httpClient;
        }

        // POST: api/Payments/process (Secured: Buyer Role Required)
        [Authorize(Roles = "Buyer")]
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentDto dto)
        {
            var buyerUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(buyerUsername)) return Unauthorized();

            // 1. Fetch current auction details from Auction Service
            AuctionDetailsDto? auction;
            try
            {
                var response = await _httpClient.GetAsync($"api/Auctions/{dto.AuctionId}");
                if (!response.IsSuccessStatusCode) return BadRequest("Auction not found or inaccessible.");

                var content = await response.Content.ReadAsStringAsync();
                auction = JsonSerializer.Deserialize<AuctionDetailsDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception)
            {
                return StatusCode(503, "Cannot communicate with Auction Service.");
            }

            // 2. Critical Validation
            if (auction?.Status != "Ended")
                return BadRequest("Auction is not ready for settlement.");
            if (auction.HighestBidderUsername != buyerUsername)
                return Unauthorized("You are not the winning bidder for this auction.");
            if (auction.SellerUsername == null)
                return BadRequest("Auction missing seller information.");

            // 3. Simulate Payment Gateway Success
            bool paymentSuccess = SimulatePayment(dto.CardNumber);

            if (!paymentSuccess)
            {
                return BadRequest(new { message = "Payment failed due to simulated card processing error." });
            }

            // 4. Record Transaction
            var transaction = new Transaction
            {
                AuctionId = dto.AuctionId,
                BuyerUsername = buyerUsername,
                SellerUsername = auction.SellerUsername,
                Amount = auction.HighestBid,
                PaymentMethod = dto.PaymentMethod,
                Status = "Completed"
            };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // 5. Update Auction Status to 'Sold' in the Auction Service
            try
            {
                var updateResponse = await _httpClient.PutAsync(
                    $"api/Auctions/status/{dto.AuctionId}?newStatus=Sold",
                    null
                );

                if (!updateResponse.IsSuccessStatusCode)
                {
                    // Log or handle consistency error
                }
            }
            catch (Exception ex)
            {
                // Log communication error with Auction Service
            }

            return Ok(new { message = "Payment successful and transaction recorded.", transaction });
        }

        // GET: api/Payments/check/{auctionId} (Used by Review Service for validation)
        [HttpGet("check/{auctionId:guid}")]
        [Authorize]
        public async Task<IActionResult> CheckTransactionExists(Guid auctionId)
        {
            var transactionExists = await _context.Transactions
                .AnyAsync(t => t.AuctionId == auctionId && t.Status == "Completed");

            if (transactionExists)
            {
                return Ok(new { exists = true });
            }
            return NotFound(new { exists = false });
        }

        // ✅ NEW ENDPOINT: GET transaction details for a specific auction
        [HttpGet("auction/{auctionId:guid}")]
        [Authorize]
        public async Task<ActionResult<Transaction>> GetTransactionForAuction(Guid auctionId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.AuctionId == auctionId);

            if (transaction == null)
            {
                return NotFound(new { message = "No transaction found for this auction." });
            }

            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            if (transaction.BuyerUsername != username && transaction.SellerUsername != username)
            {
                return Forbid("You are not authorized to view this receipt.");
            }

            return Ok(transaction);
        }

        // Simple Simulation Logic
        private bool SimulatePayment(string cardNumber)
        {
            return !cardNumber.EndsWith("0");
        }
    }
}