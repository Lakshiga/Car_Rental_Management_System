using CarRentalManagementSystem.Models;

namespace CarRentalManagementSystem.Interfaces
{
    public interface ICarRepository
    {
        Task<Car?> GetByIdAsync(int carId);
        Task UpdateAsync(Car car);
    }
}
