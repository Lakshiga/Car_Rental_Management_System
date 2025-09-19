using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarRentalManagementSystem.Enums;

namespace CarRentalManagementSystem.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }
        
        [ForeignKey("Booking")]
        public int BookingID { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal AmountPaid { get; set; }
        
        [Required]
        public DateTime PaymentDate { get; set; }
        
        [Required]
        [StringLength(20)]
        public string PaymentType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? StripePaymentIntentId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Booking Booking { get; set; } = null!;
    }
}