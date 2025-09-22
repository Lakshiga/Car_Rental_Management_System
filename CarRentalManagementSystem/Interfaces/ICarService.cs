using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Models;

namespace CarRentalManagementSystem.Services.Interfaces
{
    public interface ICarService
    {
        Task<IEnumerable<CarResponseDTO>> GetAllCarsAsync();
        Task<IEnumerable<CarResponseDTO>> GetAvailableCarsAsync();
        Task<CarResponseDTO?> GetCarByIdAsync(int carId);
        Task<bool> AddCarAsync(Car car);
        Task<bool> UpdateCarAsync(Car car);
        Task<bool> DeleteCarAsync(int carId);
        Task<bool> IsCarAvailableAsync(int carId, DateTime pickupDate, DateTime returnDate);
        Task<IEnumerable<CarResponseDTO>> SearchCarsAsync(string? searchTerm, string? carType, string? fuelType);
        Task<IEnumerable<CarResponseDTO>> SearchCarsWithDateAsync(string? searchTerm, string? carType, string? fuelType, DateTime? pickupDate, DateTime? returnDate);
    }
}