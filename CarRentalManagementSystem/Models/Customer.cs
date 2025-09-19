using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalManagementSystem.Models
{
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }
        
        [ForeignKey("User")]
        public int UserID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string NIC { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string LicenseNo { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? LicenseFrontImage { get; set; }
        
        [StringLength(500)]
        public string? LicenseBackImage { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        [StringLength(15)]
        public string PhoneNo { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? ImageUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Computed property
        public string FullName => $"{FirstName} {LastName}";
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}