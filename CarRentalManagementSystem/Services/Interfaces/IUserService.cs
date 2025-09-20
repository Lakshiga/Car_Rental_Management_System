using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Models;

namespace CarRentalManagementSystem.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterCustomerAsync(CustomerRegistrationRequestDTO request);
        Task<(bool Success, string Token, string Role, int UserId)> LoginAsync(LoginRequestDTO request);
        Task<CustomerResponseDTO?> GetCustomerByUserIdAsync(int userId);
        Task<CustomerResponseDTO?> GetCustomerByIdAsync(int customerId);
        Task<bool> UpdateCustomerAsync(int customerId, CustomerResponseDTO customer);
        Task<bool> UpdateCustomerProfileAsync(int customerId, CustomerProfileUpdateDTO profileData);
        Task<User?> GetUserByIdAsync(int userId);
        Task<IEnumerable<CustomerResponseDTO>> GetAllCustomersAsync();
        Task<IEnumerable<StaffResponseDTO>> GetAllStaffAsync();
        Task<bool> RegisterStaffAsync(StaffRegistrationRequestDTO request);
        Task<bool> ResetPasswordAsync(int userId, PasswordResetRequestDTO request);
        Task<StaffResponseDTO?> GetStaffByUserIdAsync(int userId);
        Task<bool> UpdateStaffProfileAsync(int staffId, StaffResponseDTO staffDto);
        Task<(string Username, string Password)> GenerateStaffCredentialsAsync(string email, string firstName);
    }
}