using Microsoft.EntityFrameworkCore;
using Stripe;
using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Services.Interfaces;
using CarRentalManagementSystem.Enums;

namespace CarRentalManagementSystem.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            
            // Set Stripe API key
            StripeConfiguration.ApiKey = _configuration.GetSection("StripeSettings")["SecretKey"];
        }

        public async Task<(bool Success, string? PaymentIntentId)> ProcessPaymentAsync(PaymentRequestDTO request)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(request.BookingID);
                if (booking == null)
                    return (false, null);

                // Check if this is a demo/test payment
                var isDemoPayment = IsDemoPayment(request);
                
                string paymentIntentId;
                
                if (isDemoPayment)
                {
                    // Handle demo payments without real Stripe integration
                    paymentIntentId = GenerateDemoPaymentIntent();
                }
                else
                {
                    // Create real Stripe Payment Intent
                    var options = new PaymentIntentCreateOptions
                    {
                        Amount = (long)(request.Amount * 100), // Convert to cents
                        Currency = "inr",
                        Description = $"Car Rental Payment - Booking #{request.BookingID}",
                        Metadata = new Dictionary<string, string>
                        {
                            { "booking_id", request.BookingID.ToString() },
                            { "payment_type", request.PaymentType }
                        }
                    };

                    var service = new PaymentIntentService();
                    var paymentIntent = await service.CreateAsync(options);
                    paymentIntentId = paymentIntent.Id;
                }

                // Save payment record
                var payment = new Payment
                {
                    BookingID = request.BookingID,
                    AmountPaid = request.Amount,
                    PaymentDate = DateTime.Now,
                    PaymentType = request.PaymentType,
                    PaymentStatus = isDemoPayment ? "Paid" : "Pending",
                    StripePaymentIntentId = paymentIntentId
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                
                // For demo payments, automatically update booking status
                if (isDemoPayment)
                {
                    booking.Status = "Confirmed";
                    await _context.SaveChangesAsync();
                }

                return (true, paymentIntentId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Payment Error: {ex.Message}");
                return (false, null);
            }
        }
        
        private bool IsDemoPayment(PaymentRequestDTO request)
        {
            // Check if using demo card numbers or test environment
            var demoCardNumbers = new[] { "4242424242424242", "4000000000000002", "5555555555554444" };
            return demoCardNumbers.Contains(request.CardNumber?.Replace(" ", "")) || 
                   request.CardNumber?.StartsWith("4242") == true ||
                   string.IsNullOrEmpty(request.CardNumber); // Assume demo if no card provided
        }
        
        private string GenerateDemoPaymentIntent()
        {
            return $"pi_demo_{Guid.NewGuid():N}";
        }

        public async Task<IEnumerable<PaymentResponseDTO>> GetPaymentsByBookingAsync(int bookingId)
        {
            var payments = await _context.Payments
                .Where(p => p.BookingID == bookingId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return payments.Select(MapToResponseDTO);
        }

        public async Task<IEnumerable<PaymentResponseDTO>> GetAllPaymentsAsync()
        {
            var payments = await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return payments.Select(MapToResponseDTO);
        }

        public async Task<PaymentResponseDTO?> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
                .FirstOrDefaultAsync(p => p.PaymentID == paymentId);
            return payment != null ? MapToResponseDTO(payment) : null;
        }

        public async Task<bool> ConfirmPaymentAsync(string paymentIntentId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Booking)
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);

                if (payment == null)
                    return false;

                // Check if this is a demo payment
                if (paymentIntentId.StartsWith("pi_demo_"))
                {
                    // For demo payments, automatically confirm
                    payment.PaymentStatus = "Paid";
                    payment.Booking.Status = "Confirmed";
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    // Verify payment with Stripe for real payments
                    var service = new PaymentIntentService();
                    var paymentIntent = await service.GetAsync(paymentIntentId);

                    if (paymentIntent.Status == "succeeded")
                    {
                        payment.PaymentStatus = "Paid";
                        payment.Booking.Status = "Confirmed";
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static PaymentResponseDTO MapToResponseDTO(Payment payment)
        {
            return new PaymentResponseDTO
            {
                PaymentID = payment.PaymentID,
                BookingID = payment.BookingID,
                AmountPaid = payment.AmountPaid,
                PaymentDate = payment.PaymentDate,
                PaymentStatus = payment.PaymentStatus,
                PaymentType = payment.PaymentType,
                StripePaymentIntentId = payment.StripePaymentIntentId,
                CustomerName = payment.Booking?.Customer != null ? 
                    $"{payment.Booking.Customer.FirstName} {payment.Booking.Customer.LastName}" : string.Empty
            };
        }
    }
}