using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Models;

namespace CarRentalManagementSystem.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterCustomerAsync(CustomerRegistrationRequestDTO request);
        Task<(bool Success, string Token, string Role, int UserId)> LoginAsync(LoginRequestDTO request);
        Task<CustomerResponseDTO?> GetCustomerByUserIdAsync(int userId);
        Task<bool> UpdateCustomerAsync(int customerId, CustomerResponseDTO customer);
        Task<User?> GetUserByIdAsync(int userId);
    }
}