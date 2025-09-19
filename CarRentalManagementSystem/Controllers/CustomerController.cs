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
            ViewBag.PendingBookings = bookings.Count(b => b.Status == "Pending");
            ViewBag.ApprovedBookings = bookings.Count(b => b.Status == "Approved");
            ViewBag.CompletedBookings = bookings.Count(b => b.Status == "Returned");
            ViewBag.TotalAmountSpent = bookings.Where(b => b.Status != "Rejected").Sum(b => b.TotalCost);
            
            // Recent bookings for dashboard
            ViewBag.RecentBookings = bookings.Take(5);
            
            return View();
        }
    }
}