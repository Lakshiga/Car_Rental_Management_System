using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.Services.Interfaces;

namespace CarRentalManagementSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IPaymentService _paymentService;
        private readonly IUserService _userService;

        public CustomerController(
            IBookingService bookingService,
            IPaymentService paymentService,
            IUserService userService)
        {
            _bookingService = bookingService;
            _paymentService = paymentService;
            _userService = userService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString))
                return RedirectToAction("Login", "Account");

            var customerId = int.Parse(customerIdString);
            var bookings = await _bookingService.GetBookingsByCustomerAsync(customerId);
            
            // Calculate dashboard statistics
            ViewBag.TotalBookings = bookings.Count();
            ViewBag.PendingBookings = bookings.Count(b => b.Status == "Pending" || b.Status == "Confirmed");
            ViewBag.ApprovedBookings = bookings.Count(b => b.Status == "Approved" || b.Status == "Rented");
            ViewBag.CompletedBookings = bookings.Count(b => b.Status == "Returned");
            ViewBag.TotalAmountSpent = bookings.Where(b => b.Status != "Rejected").Sum(b => b.TotalCost);
            
            // Recent bookings for dashboard
            ViewBag.RecentBookings = bookings.Take(5);
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmRent(int bookingId)
        {
            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString))
                return Json(new { success = false, message = "Please login to continue." });

            var customerId = int.Parse(customerIdString);
            
            // Verify the booking belongs to the customer
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
                return Json(new { success = false, message = "Booking not found." });

            // Check if admin has previewed the booking (status should be "Pending")
            if (booking.Status != "Pending")
                return Json(new { success = false, message = "Booking cannot be confirmed at this time." });

            var result = await _bookingService.UpdateBookingStatusAsync(bookingId, "Confirmed");
            
            if (result)
            {
                return Json(new { success = true, message = "Booking confirmed successfully! Waiting for admin approval." });
            }
            
            return Json(new { success = false, message = "Failed to confirm booking." });
        }
    }
}