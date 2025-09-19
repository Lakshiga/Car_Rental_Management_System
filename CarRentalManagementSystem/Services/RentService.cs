using Microsoft.EntityFrameworkCore;
using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Services.Interfaces;

namespace CarRentalManagementSystem.Services
{
    public class RentService : IRentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookingService _bookingService;

        public RentService(ApplicationDbContext context, IBookingService bookingService)
        {
            _context = context;
            _bookingService = bookingService;
        }

        public async Task<(bool Success, int RentId)> StartRentAsync(int bookingId, int odometerStart)
        {
            try
            {
                // Verify booking exists and is approved
                var booking = await _context.Bookings
                    .Include(b => b.Car)
                    .Include(b => b.Customer)
                    .FirstOrDefaultAsync(b => b.BookingID == bookingId);

                if (booking == null || booking.Status != "Approved")
                    return (false, 0);

                // Check if rent already exists for this booking
                var existingRent = await _context.Rents
                    .FirstOrDefaultAsync(r => r.BookingID == bookingId);

                if (existingRent != null)
                    return (false, existingRent.RentID); // Already rented

                // Create rent record
                var rent = new Rent
                {
                    BookingID = bookingId,
                    OdometerStart = odometerStart,
                    RentDate = DateTime.Now
                };

                _context.Rents.Add(rent);

                // Update booking status to "Rented"
                booking.Status = "Rented";

                // Update car status to "Rented"
                booking.Car.Status = "Rented";
                booking.Car.IsAvailable = false;

                await _context.SaveChangesAsync();

                return (true, rent.RentID);
            }
            catch
            {
                return (false, 0);
            }
        }

        public async Task<(bool Success, int ReturnId)> ProcessReturnAsync(int rentId, int odometerEnd, DateTime actualReturnDate)
        {
            try
            {
                var rent = await _context.Rents
                    .Include(r => r.Booking)
                    .ThenInclude(b => b.Car)
                    .FirstOrDefaultAsync(r => r.RentID == rentId);

                if (rent == null || rent.ActualReturnDate.HasValue)
                    return (false, 0); // Already returned

                // Calculate extra charges
                var extraCharges = await CalculateExtraChargesAsync(rentId, odometerEnd);
                var extraKm = Math.Max(0, odometerEnd - rent.OdometerStart - GetExpectedKilometers(rent.Booking));
                var totalDue = rent.Booking.TotalCost + extraCharges;

                // Create return record
                var returnRecord = new Return
                {
                    RentID = rentId,
                    OdometerEnd = odometerEnd,
                    ReturnDate = actualReturnDate,
                    ExtraKM = extraKm,
                    ExtraCharge = extraCharges,
                    TotalDue = totalDue
                };

                _context.Returns.Add(returnRecord);

                // Update rent record
                rent.OdometerEnd = odometerEnd;
                rent.ActualReturnDate = actualReturnDate;

                // Update booking status
                rent.Booking.Status = "Returned";

                // Update car status back to available
                rent.Booking.Car.Status = "Available";
                rent.Booking.Car.IsAvailable = true;

                await _context.SaveChangesAsync();

                return (true, returnRecord.ReturnID);
            }
            catch
            {
                return (false, 0);
            }
        }

        public async Task<IEnumerable<RentResponseDTO>> GetActiveRentsAsync()
        {
            var rents = await _context.Rents
                .Include(r => r.Booking)
                .ThenInclude(b => b.Car)
                .Include(r => r.Booking.Customer)
                .Where(r => !r.ActualReturnDate.HasValue)
                .OrderByDescending(r => r.RentDate)
                .ToListAsync();

            return rents.Select(MapToResponseDTO);
        }

        public async Task<IEnumerable<RentResponseDTO>> GetAllRentsAsync()
        {
            var rents = await _context.Rents
                .Include(r => r.Booking)
                .ThenInclude(b => b.Car)
                .Include(r => r.Booking.Customer)
                .Include(r => r.Return)
                .OrderByDescending(r => r.RentDate)
                .ToListAsync();

            return rents.Select(MapToResponseDTO);
        }

        public async Task<RentResponseDTO?> GetRentByIdAsync(int rentId)
        {
            var rent = await _context.Rents
                .Include(r => r.Booking)
                .ThenInclude(b => b.Car)
                .Include(r => r.Booking.Customer)
                .Include(r => r.Return)
                .FirstOrDefaultAsync(r => r.RentID == rentId);

            return rent != null ? MapToResponseDTO(rent) : null;
        }

        public async Task<RentResponseDTO?> GetRentByBookingIdAsync(int bookingId)
        {
            var rent = await _context.Rents
                .Include(r => r.Booking)
                .ThenInclude(b => b.Car)
                .Include(r => r.Booking.Customer)
                .Include(r => r.Return)
                .FirstOrDefaultAsync(r => r.BookingID == bookingId);

            return rent != null ? MapToResponseDTO(rent) : null;
        }

        public async Task<decimal> CalculateExtraChargesAsync(int rentId, int odometerEnd)
        {
            var rent = await _context.Rents
                .Include(r => r.Booking)
                .ThenInclude(b => b.Car)
                .FirstOrDefaultAsync(r => r.RentID == rentId);

            if (rent == null)
                return 0;

            var totalKmDriven = odometerEnd - rent.OdometerStart;
            var expectedKm = GetExpectedKilometers(rent.Booking);
            var extraKm = Math.Max(0, totalKmDriven - expectedKm);

            return extraKm * rent.Booking.Car.PerKmRate;
        }

        public async Task<bool> UpdateRentStatusAsync(int rentId, DateTime actualReturnDate, int odometerEnd)
        {
            try
            {
                var rent = await _context.Rents.FindAsync(rentId);
                if (rent == null)
                    return false;

                rent.ActualReturnDate = actualReturnDate;
                rent.OdometerEnd = odometerEnd;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private int GetExpectedKilometers(Booking booking)
        {
            // Calculate expected kilometers based on rental duration
            // Assuming average 100 km per day for city driving
            var rentalDays = (booking.ReturnDate - booking.PickupDate).Days;
            if (rentalDays <= 0) rentalDays = 1;
            
            return rentalDays * 100; // 100 km per day average
        }

        private RentResponseDTO MapToResponseDTO(Rent rent)
        {
            return new RentResponseDTO
            {
                RentID = rent.RentID,
                BookingID = rent.BookingID,
                BookingDetails = rent.Booking != null ? new BookingResponseDTO
                {
                    BookingID = rent.Booking.BookingID,
                    CarDetails = new CarResponseDTO
                    {
                        CarID = rent.Booking.Car.CarID,
                        CarName = rent.Booking.Car.CarName,
                        Brand = rent.Booking.Car.CarModel,
                        NumberPlate = rent.Booking.Car.NumberPlate,
                        ImageUrl = rent.Booking.Car.ImageUrl,
                        PerKmRate = rent.Booking.Car.PerKmRate
                    },
                    CustomerName = $"{rent.Booking.Customer.FirstName} {rent.Booking.Customer.LastName}",
                    PickupDate = rent.Booking.PickupDate,
                    ReturnDate = rent.Booking.ReturnDate,
                    Status = rent.Booking.Status,
                    TotalCost = rent.Booking.TotalCost
                } : null,
                OdometerStart = rent.OdometerStart,
                OdometerEnd = rent.OdometerEnd,
                RentDate = rent.RentDate,
                ActualReturnDate = rent.ActualReturnDate,
                IsReturned = rent.ActualReturnDate.HasValue,
                ExtraCharges = rent.Return?.ExtraCharge,
                TotalDue = rent.Return?.TotalDue
            };
        }
    }
}