namespace CarRentalManagementSystem.DTOs
{
    public class BookingResponseDTO
    {
        public int BookingID { get; set; }
        public CarResponseDTO CarDetails { get; set; } = new();
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string NICNumber { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string? LicenseFrontImage { get; set; }
        public string? LicenseBackImage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}