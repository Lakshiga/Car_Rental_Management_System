using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarRentalManagementSystem.Enums;

namespace CarRentalManagementSystem.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }
        
        [ForeignKey("Customer")]
        public int CustomerID { get; set; }
        
        [ForeignKey("Car")]
        public int CarID { get; set; }
        
        [Required]
        public DateTime PickupDate { get; set; }
        
        [Required]
        public DateTime ReturnDate { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalCost { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";
        
        [Required]
        [StringLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string NICNumber { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? LicenseFrontImage { get; set; }
        
        [StringLength(500)]
        public string? LicenseBackImage { get; set; }
        
        [StringLength(50)]
        public string? ApprovedBy { get; set; }
        
        public DateTime? ApprovedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual Car Car { get; set; } = null!;
        public virtual Rent? Rent { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}