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
            try
            {
                var customerIdString = HttpContext.Session.GetString("CustomerId");
                var userIdString = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");
                if (string.IsNullOrEmpty(customerIdString) || string.IsNullOrEmpty(userIdString))
                    return Json(new { success = false, message = "Please login to make a booking." });

                if (userRole != "Customer")
                    return Json(new { success = false, message = "Only customers can make bookings." });

                var customerId = int.Parse(customerIdString);
                var userId = int.Parse(userIdString);

                // Check if customer profile is complete - if not, prevent booking and ask to update profile
                var customer = await _userService.GetCustomerByUserIdAsync(userId);
                if (customer == null)
                    return Json(new { success = false, message = "Customer profile not found." });

                var profileIncomplete = string.IsNullOrEmpty(customer.NIC) ||
                                        string.IsNullOrEmpty(customer.LicenseNo) ||
                                        string.IsNullOrEmpty(customer.Phone) ||
                                        string.IsNullOrEmpty(customer.Address);

                if (profileIncomplete)
                    return Json(new { success = false, message = "Please update your profile (NIC, License, Phone, Address) before booking." });

                // If profile is complete, use existing license number unless a new one is provided
                if (string.IsNullOrEmpty(model.LicenseNumber))
                {
                    model.LicenseNumber = customer.LicenseNo;
                }

                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid booking data." });

                // Ensure dates are provided (enforce filtering first)
                if (model.PickupDate == default || model.ReturnDate == default)
                    return Json(new { success = false, message = "Please select pickup and return dates before booking." });

                // Validate dates (allow today's date, disallow past dates)
                if (model.PickupDate.Date < DateTime.Today)
                    return Json(new { success = false, message = "Pickup date cannot be in the past." });

                if (model.ReturnDate <= model.PickupDate)
                    return Json(new { success = false, message = "Return date must be after pickup date." });

                var result = await _bookingService.CreateBookingAsync(model, customerId);
                
                if (result.Success)
                {
                    // Send confirmation email
                    var customerInfo = await _userService.GetCustomerByUserIdAsync(int.Parse(HttpContext.Session.GetString("UserId")!));
                    if (customerInfo != null)
                    {
                        await _emailService.SendBookingConfirmationAsync(customerInfo.Email, customerInfo.FullName, result.BookingId);
                    }

                    return Json(new { 
                        success = true, 
                        message = "Booking successful! Redirecting to payment...", 
                        bookingId = result.BookingId,
                        redirectUrl = Url.Action("ProcessPayment", "Payment", new { bookingId = result.BookingId })
                    });
                }
                
                return Json(new { success = false, message = "Booking failed. The selected car is not available for these dates." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An unexpected error occurred while creating the booking." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateStreamlinedBooking([FromBody] StreamlinedBookingRequestDTO model)
        {
            try
            {
                var customerIdString = HttpContext.Session.GetString("CustomerId");
                var userIdString = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");
                if (string.IsNullOrEmpty(customerIdString) || string.IsNullOrEmpty(userIdString))
                    return Json(new { success = false, message = "Please login to make a booking." });

                if (userRole != "Customer")
                    return Json(new { success = false, message = "Only customers can make bookings." });

                var customerId = int.Parse(customerIdString);
                var userId = int.Parse(userIdString);

                // Ensure dates are provided (enforce filtering first)
                if (model.PickupDate == default || model.ReturnDate == default)
                    return Json(new { success = false, message = "Please select pickup and return dates before booking." });

                // Validate dates (allow today's date, disallow past dates)
                if (model.PickupDate.Date < DateTime.Today)
                    return Json(new { success = false, message = "Pickup date cannot be in the past." });

                if (model.ReturnDate <= model.PickupDate)
                    return Json(new { success = false, message = "Return date must be after pickup date." });

                // Get customer details to use existing license info
                var customer = await _userService.GetCustomerByUserIdAsync(userId);
                if (customer == null)
                    return Json(new { success = false, message = "Customer profile not found." });

                // Create booking request with existing customer data
                var bookingRequest = new CarBookingRequestDTO
                {
                    CarID = model.CarID,
                    PickupDate = model.PickupDate,
                    ReturnDate = model.ReturnDate,
                    LicenseNumber = customer.LicenseNo // Use existing license number
                };

                var result = await _bookingService.CreateBookingAsync(bookingRequest, customerId);
                
                if (result.Success)
                {
                    // Send confirmation email
                    await _emailService.SendBookingConfirmationAsync(customer.Email, customer.FullName, result.BookingId);

                    return Json(new { 
                        success = true, 
                        message = "Booking successful! Redirecting to payment...", 
                        bookingId = result.BookingId,
                        redirectUrl = Url.Action("ProcessPayment", "Payment", new { bookingId = result.BookingId })
                    });
                }
                
                return Json(new { success = false, message = "Booking failed. The selected car is not available for these dates." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An unexpected error occurred while creating the booking." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CalculateRent(int carId, DateTime pickupDate, DateTime returnDate)
        {
            try
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
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to calculate rent." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int carId, DateTime pickupDate, DateTime returnDate)
        {
            try
            {
                var isAvailable = await _carService.IsCarAvailableAsync(carId, pickupDate, returnDate);
                return Json(new { available = isAvailable });
            }
            catch (Exception)
            {
                return Json(new { available = false });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckCustomerProfileStatus()
        {
            try
            {
                var userIdString = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                    return Json(new { success = false, message = "Not logged in" });

                var userId = int.Parse(userIdString);
                var customerProfile = await _userService.GetCustomerByUserIdAsync(userId);
                
                if (customerProfile == null)
                    return Json(new { success = false, message = "Customer not found" });

                // Check if customer profile is complete
                var isComplete = !string.IsNullOrEmpty(customerProfile.NIC) && 
                               !string.IsNullOrEmpty(customerProfile.LicenseNo) && 
                               !string.IsNullOrEmpty(customerProfile.Phone) && 
                               !string.IsNullOrEmpty(customerProfile.Address);

                return Json(new { 
                    success = true, 
                    isProfileComplete = isComplete,
                    missingFields = new {
                        nic = string.IsNullOrEmpty(customerProfile.NIC),
                        licenseNumber = string.IsNullOrEmpty(customerProfile.LicenseNo),
                        phone = string.IsNullOrEmpty(customerProfile.Phone),
                        address = string.IsNullOrEmpty(customerProfile.Address)
                    },
                    existingData = new {
                        nic = customerProfile.NIC ?? "",
                        licenseNumber = customerProfile.LicenseNo ?? "",
                        phone = customerProfile.Phone ?? "",
                        address = customerProfile.Address ?? ""
                    }
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to check profile status." });
            }
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
        
        [HttpGet]
        public async Task<IActionResult> Receipt(int id)
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