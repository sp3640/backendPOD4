using System.ComponentModel.DataAnnotations;

namespace OnlineAuctionSystem.PaymentService.DTOs
{
    public class PaymentDto
    {
        [Required]
        public Guid AuctionId { get; set; }

        [Required]
        public string CardNumber { get; set; } = string.Empty; // Simulated card number

        [Required]
        public string ExpirationDate { get; set; } = string.Empty;

        [Required]
        public string CVV { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = "CreditCard";
    }
}