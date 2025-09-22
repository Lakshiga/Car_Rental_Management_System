using Microsoft.EntityFrameworkCore;
using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Interfaces;
using CarRentalManagementSystem.Models;

namespace CarRentalManagementSystem.Repositories
{
    public class CarRepository : ICarRepository
    {
        private readonly ApplicationDbContext _context;
        public CarRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Car?> GetByIdAsync(int carId)
        {
            return await _context.Cars.FindAsync(carId);
        }

        public async Task UpdateAsync(Car car)
        {
            _context.Cars.Update(car);
            await _context.SaveChangesAsync();
        }
    }
}
