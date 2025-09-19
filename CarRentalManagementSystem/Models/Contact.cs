using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.Models
{
    public class Contact
    {
        [Key]
        public int ContactID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        public bool IsReplied { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}