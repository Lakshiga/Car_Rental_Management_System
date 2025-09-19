using CarRentalManagementSystem.DTOs;

namespace CarRentalManagementSystem.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<(bool Success, string? PaymentIntentId)> ProcessPaymentAsync(PaymentRequestDTO request);
        Task<IEnumerable<PaymentResponseDTO>> GetPaymentsByBookingAsync(int bookingId);
        Task<IEnumerable<PaymentResponseDTO>> GetAllPaymentsAsync();
        Task<PaymentResponseDTO?> GetPaymentByIdAsync(int paymentId);
        Task<bool> ConfirmPaymentAsync(string paymentIntentId);
    }
}