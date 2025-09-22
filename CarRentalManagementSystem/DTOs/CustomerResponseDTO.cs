using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.DTOs
{
    public class CustomerResponseDTO
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Phone number: exactly 10 digits
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string Phone { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        // NIC: either 12 digits OR 10 digits followed by V/X (case-insensitive)
        [RegularExpression(@"^(?:\d{12}|\d{10}[VvXx])$", ErrorMessage = "NIC must be 12 digits, or 10 digits followed by 'V' or 'X'.")]
        public string NIC { get; set; } = string.Empty;
        // License number: 10 digits followed by 3 letters
        [RegularExpression(@"^\d{10}[A-Za-z]{3}$", ErrorMessage = "License number must be 10 digits followed by 3 letters.")]
        public string LicenseNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}