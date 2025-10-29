using System.ComponentModel.DataAnnotations;

namespace OnlineAuctionSystem.PaymentService.Models
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        // Links to the auction that resulted in this transaction
        public Guid AuctionId { get; set; }

        public string BuyerUsername { get; set; } = string.Empty;
        public string SellerUsername { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Completed"; // Completed, Failed, Refunded
    }
}