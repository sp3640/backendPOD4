using System.ComponentModel.DataAnnotations;

namespace OnlineAuctionSystem.BiddingService.DTOs
{
    public class BidPlaceDto
    {
        [Required]
        public Guid AuctionId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Bid amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }
}