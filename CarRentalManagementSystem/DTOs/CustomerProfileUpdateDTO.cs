namespace CarRentalManagementSystem.DTOs
{
    public class CustomerProfileUpdateDTO
    {
        public string NIC { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string LicenseNo { get; set; } = string.Empty;
    }
}