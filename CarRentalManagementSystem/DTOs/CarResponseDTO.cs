namespace CarRentalManagementSystem.DTOs
{
    public class CarResponseDTO
    {
        public int CarID { get; set; }
        public string CarName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal RentPerDay { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string CarType { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public int SeatingCapacity { get; set; }
        public double Mileage { get; set; }
        public string NumberPlate { get; set; } = string.Empty;
        public decimal PerKmRate { get; set; }
        public int AllowedKmPerDay { get; set; }
    }
}