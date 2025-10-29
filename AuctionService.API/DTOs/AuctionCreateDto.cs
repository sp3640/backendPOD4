using System.ComponentModel.DataAnnotations;

namespace OnlineAuctionSystem.AuctionService.DTOs
{
    public class AuctionCreateDto
    {
        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal StartPrice { get; set; }

        [Required]
        [Range(1, 10080, ErrorMessage = "Duration must be between 1 minute and 7 days.")]
        public int DurationMinutes { get; set; } // Duration for calculating EndTime

        public string? ImageUrl { get; set; }
    }
}