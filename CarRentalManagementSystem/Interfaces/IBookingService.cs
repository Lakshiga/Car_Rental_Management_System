using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Models;

namespace CarRentalManagementSystem.Services.Interfaces
{
    public interface IBookingService
    {
        Task<(bool Success, int BookingId)> CreateBookingAsync(CarBookingRequestDTO request, int customerId);
        Task<IEnumerable<BookingResponseDTO>> GetBookingsByCustomerAsync(int customerId);
        Task<IEnumerable<BookingResponseDTO>> GetAllBookingsAsync();
        Task<BookingResponseDTO?> GetBookingByIdAsync(int bookingId);
        Task<bool> UpdateBookingStatusAsync(int bookingId, string status);
        Task<decimal> CalculateRentalCostAsync(int carId, DateTime pickupDate, DateTime returnDate);
        Task<bool> ConfirmBookingAsync(int bookingId);
        Task<bool> ApproveBookingAsync(int bookingId, string approvedBy);
        Task<(bool Success, string? Message)> RejectBookingAsync(int bookingId, string rejectedBy, string rejectionReason);
        Task<(bool Success, int RentId)> StartRentAsync(int bookingId, int odometerStart);
    }
}