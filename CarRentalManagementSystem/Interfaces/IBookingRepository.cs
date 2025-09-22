using CarRentalManagementSystem.Models;
using System.Linq.Expressions;

namespace CarRentalManagementSystem.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(int bookingId);
        Task<List<Booking>> GetAllAsync();
        Task<List<Booking>> GetByCustomerAsync(int customerId);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);

        // Rent related
        Task<Rent?> GetRentByBookingIdAsync(int bookingId);
        Task AddRentAsync(Rent rent);
    }
}
