using CarRentalManagementSystem.Models;

namespace CarRentalManagementSystem.Interfaces
{
    public interface IUserRepository
    {
        Task<Customer?> GetCustomerByIdAsync(int customerId);
    }
}
