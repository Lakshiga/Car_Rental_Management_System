using Microsoft.EntityFrameworkCore;
using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Interfaces;
using CarRentalManagementSystem.Models;

namespace CarRentalManagementSystem.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;
        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByIdAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetByCustomerAsync(int customerId)
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .Where(b => b.CustomerID == customerId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<Rent?> GetRentByBookingIdAsync(int bookingId)
        {
            return await _context.Rents.FirstOrDefaultAsync(r => r.BookingID == bookingId);
        }

        public async Task AddRentAsync(Rent rent)
        {
            _context.Rents.Add(rent);
            await _context.SaveChangesAsync();
        }
    }
}
