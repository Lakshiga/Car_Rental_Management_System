using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.DTOs
{
    public class PaymentRequestDTO
    {
        [Required]
        public int BookingID { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        public string PaymentType { get; set; } = string.Empty;
        
        public string? StripeToken { get; set; }
    }
}