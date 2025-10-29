using System.ComponentModel.DataAnnotations;

namespace OnlineAuctionSystem.ReviewService.DTOs
{
    public class ReviewCreateDto
    {
        [Required]
        public Guid AuctionId { get; set; } // Identifies the transaction context

        [Required]
        // The username of the person being reviewed (Seller or Buyer)
        public string ReviewedUsername { get; set; } = string.Empty;

        [Required]
        public string ReviewType { get; set; } = string.Empty; // "Seller" or "Buyer"

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required]
        [StringLength(500)]
        public string Comment { get; set; } = string.Empty;
    }
}