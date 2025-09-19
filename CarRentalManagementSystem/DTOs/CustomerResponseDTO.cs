namespace CarRentalManagementSystem.DTOs
{
    public class CustomerResponseDTO
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public string NIC { get; set; } = string.Empty;
        public string LicenseNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}