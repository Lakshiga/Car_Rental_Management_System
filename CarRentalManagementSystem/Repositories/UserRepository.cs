using Microsoft.EntityFrameworkCore;
using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Interfaces;
using CarRentalManagementSystem.Models;

namespace CarRentalManagementSystem.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == customerId);
        }
    }
}
