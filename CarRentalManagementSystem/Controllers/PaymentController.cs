using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Services.Interfaces;

namespace CarRentalManagementSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IPaymentService paymentService,
            IBookingService bookingService,
            IConfiguration configuration)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> ProcessPayment(int bookingId)
        {
            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString))
                return RedirectToAction("Login", "Account");

            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
                return NotFound();

            // Check if user owns this booking
            var customerId = int.Parse(customerIdString);
            if (booking.CustomerName != HttpContext.Session.GetString("CustomerName"))
                return Forbid();

            ViewBag.StripePublishableKey = _configuration.GetSection("StripeSettings")["PublishableKey"];
            ViewBag.BookingId = bookingId;
            ViewBag.Amount = booking.TotalCost * 0.5m; // 50% advance payment

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequestDTO request)
        {
            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString))
                return Json(new { success = false, message = "Please login to continue." });

            var result = await _paymentService.ProcessPaymentAsync(request);
            
            if (result.Success)
            {
                return Json(new { 
                    success = true, 
                    clientSecret = result.PaymentIntentId,
                    message = "Payment intent created successfully." 
                });
            }
            
            return Json(new { success = false, message = "Failed to create payment intent." });
        }

        [HttpPost]
        public IActionResult ConfirmPayment(string paymentIntentId)
        {
            // Temporary fix: Remove call to missing method and show error
            TempData["ErrorMessage"] = "Payment confirmation is not implemented.";
            return RedirectToAction("MyBookings", "Booking");
        }

        public async Task<IActionResult> PaymentHistory()
        {
            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString))
                return RedirectToAction("Login", "Account");

            // Get all payments for customer's bookings
            var payments = await _paymentService.GetAllPaymentsAsync();
            
            // Filter by customer (this would need to be improved with proper customer filtering)
            return View(payments);
        }
    }
}