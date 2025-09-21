namespace CarRentalManagementSystem.DTOs
{
    public class RentResponseDTO
    {
        public int RentID { get; set; }
        public int BookingID { get; set; }
        public BookingResponseDTO? BookingDetails { get; set; }
        public int OdometerStart { get; set; }
        public int? OdometerEnd { get; set; }
        public DateTime RentDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public bool IsReturned { get; set; }
        public decimal? ExtraCharges { get; set; }
        public decimal? TotalDue { get; set; }
    }

    public class StartRentRequestDTO
    {
        public int BookingID { get; set; }
        public int OdometerStart { get; set; }
        public DateTime RentDate { get; set; } = DateTime.Now;
    }

    public class ProcessReturnRequestDTO
    {
        public int RentID { get; set; }
        public int OdometerEnd { get; set; }
        public DateTime ActualReturnDate { get; set; } = DateTime.Now;
        public bool HasDamage { get; set; }
        public string? DamageReason { get; set; }
        public decimal? DamageAmount { get; set; }
    }
}