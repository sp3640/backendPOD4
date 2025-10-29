using System.ComponentModel.DataAnnotations;

namespace OnlineAuctionSystem.AuctionService.Models
{
    public class Auction
    {
        // Primary Key
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Links to the seller via their username from the Identity Service
        public string SellerUsername { get; set; } = string.Empty;

        [Required]
        public decimal StartPrice { get; set; }

        public string Status { get; set; } = "Live"; // Live, Ended, Sold, Cancelled

        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime EndTime { get; set; }

        // Fields to track bidding activity (updated via Bidding Service)
        public decimal HighestBid { get; set; }
        public string? HighestBidderUsername { get; set; }

        // ... inside Auction class
       // public string? HighestBidderUsername { get; set; }

        // ADD THIS LINE
        public string? ImageUrl { get; set; } // Will store the primary image URL
    }
}