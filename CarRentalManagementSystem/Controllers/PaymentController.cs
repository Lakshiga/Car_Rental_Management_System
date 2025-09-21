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

        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var payments = await _paymentService.GetAllPaymentsAsync();
            ViewBag.UserRole = userRole;
            return View(payments);
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

        [HttpGet]
        public async Task<IActionResult> ConfirmPayment(string paymentIntentId)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId))
            {
                TempData["ErrorMessage"] = "Invalid payment reference.";
                return RedirectToAction("MyBookings", "Booking");
            }

            var success = await _paymentService.ConfirmPaymentAsync(paymentIntentId);
            if (success)
            {
                TempData["SuccessMessage"] = "Payment confirmed successfully! Your booking is confirmed.";
                return RedirectToAction("Dashboard", "Customer");
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to confirm the payment. If you were charged, please contact support.";
                return RedirectToAction("MyBookings", "Booking");
            }
        }

        public async Task<IActionResult> PaymentHistory()
        {
            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString))
                return RedirectToAction("Login", "Account");

            // Get all payments for customer's bookings
            var customerId = int.Parse(customerIdString);
            var bookings = await _bookingService.GetBookingsByCustomerAsync(customerId);
            var bookingIds = bookings.Select(b => b.BookingID).ToHashSet();

            var allPayments = await _paymentService.GetAllPaymentsAsync();
            var payments = allPayments.Where(p => bookingIds.Contains(p.BookingID));

            return View(payments);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentDetails(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            return PartialView("~/Views/Payment/_PaymentDetailsPartial.cshtml", payment);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerPaymentDetails(int id)
        {
            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString))
                return RedirectToAction("Login", "Account");

            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            // Verify that this payment belongs to the current customer
            var customerId = int.Parse(customerIdString);
            var bookings = await _bookingService.GetBookingsByCustomerAsync(customerId);
            var bookingIds = bookings.Select(b => b.BookingID).ToHashSet();

            if (!bookingIds.Contains(payment.BookingID))
                return Forbid();

            return PartialView("~/Views/Payment/_CustomerPaymentDetailsPartial.cshtml", payment);
        }
    }
}