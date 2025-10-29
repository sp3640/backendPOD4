using System.ComponentModel.DataAnnotations;

namespace OnlineAuctionSystem.ReviewService.Models
{
    public class Review
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AuctionId { get; set; }

        // The user submitting the review (The Buyer for Seller reviews, or Seller for Buyer reviews)
        public string ReviewerUsername { get; set; } = string.Empty;

        // The user receiving the review
        public string ReviewedUsername { get; set; } = string.Empty;

        public string ReviewType { get; set; } = string.Empty;

        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}