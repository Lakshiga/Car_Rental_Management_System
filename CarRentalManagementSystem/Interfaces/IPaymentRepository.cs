using CarRentalManagementSystem.Models;

namespace CarRentalManagementSystem.Interfaces
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<List<Payment>> GetByBookingAsync(int bookingId);
    }
}
