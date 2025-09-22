using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Services.Interfaces;
using CarRentalManagementSystem.Enums;
using CarRentalManagementSystem.Interfaces;

namespace CarRentalManagementSystem.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICarRepository _carRepo;
        private readonly ICarService _carService;

        public BookingService(IBookingRepository bookingRepo, IPaymentRepository paymentRepo, IUserRepository userRepo, ICarRepository carRepo, ICarService carService)
        {
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
            _userRepo = userRepo;
            _carRepo = carRepo;
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
                var customer = await _userRepo.GetCustomerByIdAsync(customerId);
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

                await _bookingRepo.AddAsync(booking);

                return (true, booking.BookingID);
            }
            catch
            {
                return (false, 0);
            }
        }

        public async Task<IEnumerable<BookingResponseDTO>> GetBookingsByCustomerAsync(int customerId)
        {
            var bookings = await _bookingRepo.GetByCustomerAsync(customerId);

            return bookings.Select(MapToResponseDTO);
        }

        public async Task<IEnumerable<BookingResponseDTO>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepo.GetAllAsync();

            return bookings.Select(MapToResponseDTO);
        }

        public async Task<BookingResponseDTO?> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);

            return booking != null ? MapToResponseDTO(booking) : null;
        }

        public async Task<bool> UpdateBookingStatusAsync(int bookingId, string status)
        {
            try
            {
                var booking = await _bookingRepo.GetByIdAsync(bookingId);
                if (booking == null)
                    return false;

                booking.Status = status;
                await _bookingRepo.UpdateAsync(booking);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<decimal> CalculateRentalCostAsync(int carId, DateTime pickupDate, DateTime returnDate)
        {
            var car = await _carRepo.GetByIdAsync(carId);
            if (car == null)
                return 0;

            var totalDays = (returnDate - pickupDate).Days;
            if (totalDays <= 0)
                totalDays = 1;

            return totalDays * car.RentPerDay;
        }

        public async Task<bool> ConfirmBookingAsync(int bookingId)
        {
            try
            {
                var booking = await _bookingRepo.GetByIdAsync(bookingId);
                if (booking == null || booking.Status != "Pending")
                    return false;

                booking.Status = "Confirmed";
                await _bookingRepo.UpdateAsync(booking);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ApproveBookingAsync(int bookingId, string approvedBy)
        {
            try
            {
                var booking = await _bookingRepo.GetByIdAsync(bookingId);
                if (booking == null || booking.Status != "Confirmed")
                    return false;

                booking.Status = "Approved";
                booking.ApprovedBy = approvedBy;
                booking.ApprovedAt = DateTime.Now;
                await _bookingRepo.UpdateAsync(booking);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool Success, string? Message)> RejectBookingAsync(int bookingId, string rejectedBy, string rejectionReason)
        {
            try
            {
                // Get the booking
                var booking = await _bookingRepo.GetByIdAsync(bookingId);
                
                if (booking == null)
                {
                    return (false, "Booking not found.");
                }

                // Calculate advance payment amount (assumed 50%)
                var advanceAmount = booking.TotalCost * 0.5m;
                
                // Update booking status (only if not already rejected)
                if (booking.Status != "Rejected")
                {
                    booking.Status = "Rejected";
                    booking.ApprovedBy = $"Rejected by {rejectedBy} - Reason: {rejectionReason}";
                    booking.ApprovedAt = DateTime.Now;
                    
                    await _bookingRepo.UpdateAsync(booking);
                }

                // Record refund as a negative payment to affect revenue calculations
                var refundPayment = new Payment
                {
                    BookingID = bookingId,
                    AmountPaid = -advanceAmount,
                    PaymentDate = DateTime.Now,
                    PaymentType = "Refund",
                    PaymentStatus = "Refunded"
                };

                await _paymentRepo.AddAsync(refundPayment);

                return (true, $"Refunded successfully! Advance payment of ₹{advanceAmount:F2} has been deducted from revenue.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RejectBookingAsync Error: {ex.Message}");
                return (false, "Failed to reject booking and process refund.");
            }
        }

        public async Task<(bool Success, int RentId)> StartRentAsync(int bookingId, int odometerStart)
        {
            try
            {
                // Verify booking exists and is approved
                var booking = await _bookingRepo.GetByIdAsync(bookingId);

                if (booking == null || booking.Status != "Approved")
                    return (false, 0);

                // Check if rent already exists for this booking
                var existingRent = await _bookingRepo.GetRentByBookingIdAsync(bookingId);

                if (existingRent != null)
                    return (false, existingRent.RentID); // Already rented

                // Create rent record
                var rent = new Rent
                {
                    BookingID = bookingId,
                    OdometerStart = odometerStart,
                    RentDate = DateTime.Now
                };

                await _bookingRepo.AddRentAsync(rent);

                // Update booking status to "Rented"
                booking.Status = "Rented";

                // Update car status to "Rented"
                if (booking.Car != null)
                {
                    booking.Car.Status = "Rented";
                    booking.Car.IsAvailable = false;
                    await _carRepo.UpdateAsync(booking.Car);
                }

                await _bookingRepo.UpdateAsync(booking);

                return (true, rent.RentID);
            }
            catch
            {
                return (false, 0);
            }
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
                    PerKmRate = booking.Car.PerKmRate,
                    AllowedKmPerDay = booking.Car.AllowedKmPerDay
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
                ApprovedBy = booking.ApprovedBy,
                ApprovedAt = booking.ApprovedAt,
                CreatedAt = booking.CreatedAt
            };
        }
    }
}