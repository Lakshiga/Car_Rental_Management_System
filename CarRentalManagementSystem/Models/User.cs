using System.ComponentModel.DataAnnotations;
using CarRentalManagementSystem.Enums;

namespace CarRentalManagementSystem.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty;
        
        public bool RequirePasswordReset { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual Staff? Staff { get; set; }
    }
}