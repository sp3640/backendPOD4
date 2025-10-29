using System.ComponentModel.DataAnnotations;

namespace OnlineAuctionSystem.BiddingService.Models
{
    public class Bid
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        // Foreign key to the auction record in the Auction Service (Guid is used for consistency)
        public Guid AuctionId { get; set; }

        // Links to the bidder via their username from the Identity Service
        public string BidderUsername { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}