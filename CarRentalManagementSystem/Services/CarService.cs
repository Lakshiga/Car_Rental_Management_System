using Microsoft.EntityFrameworkCore;
using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Services.Interfaces;
using CarRentalManagementSystem.Enums;

namespace CarRentalManagementSystem.Services
{
    public class CarService : ICarService
    {
        private readonly ApplicationDbContext _context;

        public CarService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CarResponseDTO>> GetAllCarsAsync()
        {
            var cars = await _context.Cars.ToListAsync();
            return cars.Select(MapToResponseDTO);
        }

        public async Task<IEnumerable<CarResponseDTO>> GetAvailableCarsAsync()
        {
            var cars = await _context.Cars
                .Where(c => c.IsAvailable && c.Status == "Available")
                .ToListAsync();
            return cars.Select(MapToResponseDTO);
        }

        public async Task<CarResponseDTO?> GetCarByIdAsync(int carId)
        {
            var car = await _context.Cars.FindAsync(carId);
            return car != null ? MapToResponseDTO(car) : null;
        }

        public async Task<bool> AddCarAsync(Car car)
        {
            try
            {
                _context.Cars.Add(car);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCarAsync(Car car)
        {
            try
            {
                _context.Cars.Update(car);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCarAsync(int carId)
        {
            try
            {
                var car = await _context.Cars.FindAsync(carId);
                if (car == null)
                    return false;

                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsCarAvailableAsync(int carId, DateTime pickupDate, DateTime returnDate)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null || !car.IsAvailable)
                return false;

            // Check for overlapping bookings
            var overlappingBookings = await _context.Bookings
                .Where(b => b.CarID == carId && 
                           b.Status != "Rejected" && 
                           b.Status != "Returned" &&
                           ((b.PickupDate <= pickupDate && b.ReturnDate >= pickupDate) ||
                            (b.PickupDate <= returnDate && b.ReturnDate >= returnDate) ||
                            (b.PickupDate >= pickupDate && b.ReturnDate <= returnDate)))
                .AnyAsync();

            return !overlappingBookings;
        }

        public async Task<IEnumerable<CarResponseDTO>> SearchCarsAsync(string? searchTerm, string? carType, string? fuelType)
        {
            var query = _context.Cars.Where(c => c.IsAvailable && c.Status == "Available");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.CarName.Contains(searchTerm) || 
                                        c.CarModel.Contains(searchTerm) ||
                                        c.NumberPlate.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(carType))
            {
                query = query.Where(c => c.CarType == carType);
            }

            if (!string.IsNullOrEmpty(fuelType))
            {
                query = query.Where(c => c.FuelType == fuelType);
            }

            var cars = await query.ToListAsync();
            return cars.Select(MapToResponseDTO);
        }

        private static CarResponseDTO MapToResponseDTO(Car car)
        {
            return new CarResponseDTO
            {
                CarID = car.CarID,
                CarName = car.CarName,
                Brand = car.CarModel,
                RentPerDay = car.RentPerDay,
                AvailabilityStatus = car.Status,
                ImageUrl = car.ImageUrl,
                CarType = car.CarType,
                FuelType = car.FuelType,
                SeatingCapacity = car.SeatingCapacity,
                Mileage = car.Mileage,
                NumberPlate = car.NumberPlate,
                PerKmRate = car.PerKmRate
            };
        }
    }
}