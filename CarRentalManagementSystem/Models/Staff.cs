using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalManagementSystem.Models
{
    public class Staff
    {
        [Key]
        public int StaffID { get; set; }
        
        [ForeignKey("User")]
        public int UserID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string NIC { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? ImageUrl { get; set; }
        
        public bool IsProfileComplete { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}