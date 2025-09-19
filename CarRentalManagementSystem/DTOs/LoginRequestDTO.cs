using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.DTOs
{
    public class LoginRequestDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}