namespace CarRentalManagementSystem.DTOs
{
    public class PaymentResponseDTO
    {
        public int PaymentID { get; set; }
        public int BookingID { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string? StripePaymentIntentId { get; set; }
    }
}