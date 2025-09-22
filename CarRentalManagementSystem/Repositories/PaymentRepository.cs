using Microsoft.EntityFrameworkCore;
using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Interfaces;
using CarRentalManagementSystem.Models;

namespace CarRentalManagementSystem.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Payment>> GetByBookingAsync(int bookingId)
        {
            return await _context.Payments
                .Where(p => p.BookingID == bookingId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
    }
}
