using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Models;

namespace CarRentalManagementSystem.Services.Interfaces
{
    public interface IRentService
    {
        Task<(bool Success, int RentId)> StartRentAsync(int bookingId, int odometerStart);
        Task<(bool Success, int ReturnId)> ProcessReturnAsync(int rentId, int odometerEnd, DateTime actualReturnDate, bool hasDamage = false, string? damageReason = null, decimal damageAmount = 0);
        Task<IEnumerable<RentResponseDTO>> GetActiveRentsAsync();
        Task<IEnumerable<RentResponseDTO>> GetAllRentsAsync();
        Task<RentResponseDTO?> GetRentByIdAsync(int rentId);
        Task<RentResponseDTO?> GetRentByBookingIdAsync(int bookingId);
        Task<decimal> CalculateExtraChargesAsync(int rentId, int odometerEnd, DateTime actualReturnDate);
        Task<bool> UpdateRentStatusAsync(int rentId, DateTime actualReturnDate, int odometerEnd);
        Task<bool> ProcessFinalPaymentAsync(int returnId);
        Task<IEnumerable<Return>> GetPendingPaymentsAsync();
    }
}