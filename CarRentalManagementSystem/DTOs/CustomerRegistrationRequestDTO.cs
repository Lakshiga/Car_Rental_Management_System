using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.DTOs
{
    public class CustomerRegistrationRequestDTO
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string NIC { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string LicenseNo { get; set; } = string.Empty;
        
        [Required]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}