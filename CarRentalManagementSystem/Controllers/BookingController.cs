using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Services.Interfaces;

namespace CarRentalManagementSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ICarService _carService;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;

        public BookingController(
            IBookingService bookingService, 
            ICarService carService,
            IEmailService emailService,
            IUserService userService)
        {
            _bookingService = bookingService;
            _carService = carService;
            _emailService = emailService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(CarBookingRequestDTO model)
        {
            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString))
                return Json(new { success = false, message = "Please login to make a booking." });

            var customerId = int.Parse(customerIdString);

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid booking data." });

            // Validate dates
            if (model.PickupDate <= DateTime.Now)
                return Json(new { success = false, message = "Pickup date must be in the future." });

            if (model.ReturnDate <= model.PickupDate)
                return Json(new { success = false, message = "Return date must be after pickup date." });

            var result = await _bookingService.CreateBookingAsync(model, customerId);
            
            if (result.Success)
            {
                // Send confirmation email
                var customer = await _userService.GetCustomerByUserIdAsync(int.Parse(HttpContext.Session.GetString("UserId")!));
                if (customer != null)
                {
                    await _emailService.SendBookingConfirmationAsync(customer.Email, customer.FullName, result.BookingId);
                }

                return Json(new { 
                    success = true, 
                    message = "Booking successful! Redirecting to payment...", 
                    bookingId = result.BookingId,
                    redirectUrl = Url.Action("ProcessPayment", "Payment", new { bookingId = result.BookingId })
                });
            }
            
            return Json(new { success = false, message = "Booking failed. Car might not be available for selected dates." });
        }

        [HttpGet]
        public async Task<IActionResult> CalculateRent(int carId, DateTime pickupDate, DateTime returnDate)
        {
            if (pickupDate >= returnDate)
                return Json(new { success = false, message = "Invalid dates" });

            var totalCost = await _bookingService.CalculateRentalCostAsync(carId, pickupDate, returnDate);
            var totalDays = (returnDate - pickupDate).Days;
            var advancePayment = totalCost * 0.5m; // 50% advance

            return Json(new { 
                success = true, 
                totalDays = totalDays,
                totalCost = totalCost,
                advancePayment = advancePayment
            });
        }

        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int carId, DateTime pickupDate, DateTime returnDate)
        {
            var isAvailable = await _carService.IsCarAvailableAsync(carId, pickupDate, returnDate);
            return Json(new { available = isAvailable });
        }

        public async Task<IActionResult> MyBookings()
        {
            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString))
                return RedirectToAction("Login", "Account");

            var customerId = int.Parse(customerIdString);
            var bookings = await _bookingService.GetBookingsByCustomerAsync(customerId);
            
            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
                return NotFound();

            // Check if user owns this booking or is admin/staff
            var userRole = HttpContext.Session.GetString("UserRole");
            var customerIdString = HttpContext.Session.GetString("CustomerId");
            
            if (userRole != "Admin" && userRole != "Staff")
            {
                if (string.IsNullOrEmpty(customerIdString) || 
                    booking.CustomerName != HttpContext.Session.GetString("CustomerName"))
                {
                    return Forbid();
                }
            }

            return View(booking);
        }
    }
}