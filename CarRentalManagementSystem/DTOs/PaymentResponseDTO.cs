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
        
        // Additional properties for admin view
        public string CustomerName { get; set; } = string.Empty;
        public string PaymentMethod => PaymentType;
        public string Status => PaymentStatus;
        public decimal Amount => AmountPaid;
    }
}