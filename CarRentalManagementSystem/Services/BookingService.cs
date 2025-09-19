using Microsoft.EntityFrameworkCore;
using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Services.Interfaces;
using CarRentalManagementSystem.Enums;

namespace CarRentalManagementSystem.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICarService _carService;

        public BookingService(ApplicationDbContext context, ICarService carService)
        {
            _context = context;
            _carService = carService;
        }

        public async Task<(bool Success, int BookingId)> CreateBookingAsync(CarBookingRequestDTO request, int customerId)
        {
            try
            {
                // Validate car availability
                if (!await _carService.IsCarAvailableAsync(request.CarID, request.PickupDate, request.ReturnDate))
                    return (false, 0);

                // Calculate total cost
                var totalCost = await CalculateRentalCostAsync(request.CarID, request.PickupDate, request.ReturnDate);

                // Get customer details
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return (false, 0);

                var booking = new Booking
                {
                    CustomerID = customerId,
                    CarID = request.CarID,
                    PickupDate = request.PickupDate,
                    ReturnDate = request.ReturnDate,
                    TotalCost = totalCost,
                    Status = "Pending",
                    LicenseNumber = request.LicenseNumber,
                    NICNumber = customer.NIC,
                    LicenseFrontImage = await SaveImageAsync(request.LicenseFrontImage),
                    LicenseBackImage = await SaveImageAsync(request.LicenseBackImage)
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return (true, booking.BookingID);
            }
            catch
            {
                return (false, 0);
            }
        }

        public async Task<IEnumerable<BookingResponseDTO>> GetBookingsByCustomerAsync(int customerId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .Where(b => b.CustomerID == customerId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(MapToResponseDTO);
        }

        public async Task<IEnumerable<BookingResponseDTO>> GetAllBookingsAsync()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(MapToResponseDTO);
        }

        public async Task<BookingResponseDTO?> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            return booking != null ? MapToResponseDTO(booking) : null;
        }

        public async Task<bool> UpdateBookingStatusAsync(int bookingId, string status)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                    return false;

                booking.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<decimal> CalculateRentalCostAsync(int carId, DateTime pickupDate, DateTime returnDate)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null)
                return 0;

            var totalDays = (returnDate - pickupDate).Days;
            if (totalDays <= 0)
                totalDays = 1;

            return totalDays * car.RentPerDay;
        }

        public async Task<bool> ApproveBookingAsync(int bookingId)
        {
            return await UpdateBookingStatusAsync(bookingId, "Approved");
        }

        public async Task<bool> RejectBookingAsync(int bookingId)
        {
            return await UpdateBookingStatusAsync(bookingId, "Rejected");
        }

        private async Task<string?> SaveImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "licenses");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return "/uploads/licenses/" + uniqueFileName;
            }
            catch
            {
                return null;
            }
        }

        private static BookingResponseDTO MapToResponseDTO(Booking booking)
        {
            return new BookingResponseDTO
            {
                BookingID = booking.BookingID,
                CarDetails = new CarResponseDTO
                {
                    CarID = booking.Car.CarID,
                    CarName = booking.Car.CarName,
                    Brand = booking.Car.CarModel,
                    RentPerDay = booking.Car.RentPerDay,
                    AvailabilityStatus = booking.Car.Status,
                    ImageUrl = booking.Car.ImageUrl,
                    CarType = booking.Car.CarType,
                    FuelType = booking.Car.FuelType,
                    SeatingCapacity = booking.Car.SeatingCapacity,
                    Mileage = booking.Car.Mileage,
                    NumberPlate = booking.Car.NumberPlate,
                    PerKmRate = booking.Car.PerKmRate
                },
                PickupDate = booking.PickupDate,
                ReturnDate = booking.ReturnDate,
                TotalCost = booking.TotalCost,
                Status = booking.Status,
                CustomerName = $"{booking.Customer.FirstName} {booking.Customer.LastName}",
                NICNumber = booking.NICNumber,
                LicenseNumber = booking.LicenseNumber,
                LicenseFrontImage = booking.LicenseFrontImage,
                LicenseBackImage = booking.LicenseBackImage,
                CreatedAt = booking.CreatedAt
            };
        }
    }
}