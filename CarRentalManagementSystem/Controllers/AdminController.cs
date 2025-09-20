using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.Services.Interfaces;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Enums;

namespace CarRentalManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ICarService _carService;
        private readonly IBookingService _bookingService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public AdminController(
            ICarService carService,
            IBookingService bookingService,
            IUserService userService,
            IEmailService emailService)
        {
            _carService = carService;
            _bookingService = bookingService;
            _userService = userService;
            _emailService = emailService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var cars = await _carService.GetAllCarsAsync();
            var bookings = await _bookingService.GetAllBookingsAsync();

            ViewBag.TotalFleet = cars.Count();
            ViewBag.AvailableCars = cars.Count(c => c.AvailabilityStatus == "Available");
            ViewBag.TotalBookings = bookings.Count();
            ViewBag.PendingBookings = bookings.Count(b => b.Status == "Pending");
            ViewBag.UserRole = userRole;

            return View();
        }

        public async Task<IActionResult> Cars()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var cars = await _carService.GetAllCarsAsync();
            ViewBag.UserRole = userRole;
            return View(cars);
        }

        [HttpGet]
        public IActionResult AddCar()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Dashboard");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCar(Car model, IFormFile? imageFile)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Dashboard");

            // Set CarBrand from CarModel field (form uses CarModel for brand input)
            model.CarBrand = model.CarModel;
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "cars");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                model.ImageUrl = "/images/cars/" + uniqueFileName;
            }

            // Ensure Status is set correctly
            model.Status = model.IsAvailable ? "Available" : "Unavailable";
            
            var result = await _carService.AddCarAsync(model);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Car added successfully!";
                return RedirectToAction("Cars");
            }
            
            ModelState.AddModelError("", "Failed to add car. Please try again.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCar(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Dashboard");

            var car = await _carService.GetCarByIdAsync(id);
            if (car == null)
                return NotFound();

            var carModel = new Car
            {
                CarID = car.CarID,
                CarName = car.CarName,
                CarModel = car.Brand,
                CarBrand = car.Brand,
                ImageUrl = car.ImageUrl,
                IsAvailable = car.AvailabilityStatus == "Available",
                RentPerDay = car.RentPerDay,
                PerKmRate = car.PerKmRate,
                CarType = car.CarType,
                FuelType = car.FuelType,
                SeatingCapacity = car.SeatingCapacity,
                Mileage = car.Mileage,
                NumberPlate = car.NumberPlate,
                Status = car.AvailabilityStatus
            };

            return View(carModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditCar(Car model, IFormFile? imageFile)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Dashboard");

            if (!ModelState.IsValid)
                return View(model);

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "cars");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                model.ImageUrl = "/images/cars/" + uniqueFileName;
            }

            var result = await _carService.UpdateCarAsync(model);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Car updated successfully!";
                return RedirectToAction("Cars");
            }
            
            ModelState.AddModelError("", "Failed to update car.");
            return View(model);
        }

        public async Task<IActionResult> Bookings()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var bookings = await _bookingService.GetAllBookingsAsync();
            ViewBag.UserRole = userRole;
            return View(bookings);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var model = new DTOs.CustomerResponseDTO
            {
                FullName = HttpContext.Session.GetString("CustomerName") ?? string.Empty,
                Role = userRole ?? string.Empty
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(DTOs.CustomerResponseDTO model, IFormFile? imageFile)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                HttpContext.Session.SetString("AvatarUrl", "/images/profiles/" + uniqueFileName);
            }

            if (!string.IsNullOrWhiteSpace(model.FullName))
                HttpContext.Session.SetString("DisplayName", model.FullName);

            TempData["SuccessMessage"] = "Profile updated.";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveBooking(int bookingId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Json(new { success = false, message = "Unauthorized" });

            var username = HttpContext.Session.GetString("DisplayName") ?? "Unknown";
            var approvedByText = $"{userRole} ({username})";
            
            var result = await _bookingService.ApproveBookingAsync(bookingId, approvedByText);
            
            if (result)
            {
                // Send approval email
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (booking != null)
                {
                    var customer = await _userService.GetCustomerByUserIdAsync(
                        (await _userService.GetUserByIdAsync(booking.BookingID))?.UserID ?? 0);
                    
                    if (customer != null)
                    {
                        await _emailService.SendBookingApprovalAsync(customer.Email, customer.FullName, bookingId);
                    }
                }
                
                return Json(new { success = true, message = "Booking approved successfully!" });
            }
            
            return Json(new { success = false, message = "Failed to approve booking." });
        }

        [HttpPost]
        public async Task<IActionResult> RejectBooking(int bookingId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Json(new { success = false, message = "Unauthorized" });

            var username = HttpContext.Session.GetString("DisplayName") ?? "Unknown";
            var rejectedByText = $"{userRole} ({username})";
            
            var result = await _bookingService.RejectBookingAsync(bookingId, rejectedByText);
            
            if (result)
            {
                // Send rejection email
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (booking != null)
                {
                    var customer = await _userService.GetCustomerByUserIdAsync(
                        (await _userService.GetUserByIdAsync(booking.BookingID))?.UserID ?? 0);
                    
                    if (customer != null)
                    {
                        await _emailService.SendBookingRejectionAsync(customer.Email, customer.FullName, bookingId);
                    }
                }
                
                return Json(new { success = true, message = "Booking rejected successfully!" });
            }
            
            return Json(new { success = false, message = "Failed to reject booking." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Json(new { success = false, message = "Unauthorized" });

            var result = await _carService.DeleteCarAsync(id);
            
            if (result)
                return Json(new { success = true, message = "Car deleted successfully!" });
            
            return Json(new { success = false, message = "Failed to delete car." });
        }

        public async Task<IActionResult> Customers()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Login", "Account");

            var customers = await _userService.GetAllCustomersAsync();
            ViewBag.UserRole = userRole;
            return View(customers);
        }

        public async Task<IActionResult> Staff()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Login", "Account");

            var staff = await _userService.GetAllStaffAsync();
            ViewBag.UserRole = userRole;
            return View(staff);
        }

        [HttpPost]
        public async Task<IActionResult> AddStaff([FromBody] DTOs.StaffRegistrationRequestDTO request)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return Json(new { success = false, errors = errors });
            }

            try
            {
                // Generate credentials first
                var (username, password) = await _userService.GenerateStaffCredentialsAsync(request.Email, request.FirstName);
                
                // Register staff
                var result = await _userService.RegisterStaffAsync(request);
                
                if (result)
                {
                    // Send email with credentials
                    var emailResult = await _emailService.SendStaffCredentialsAsync(
                        request.Email, 
                        $"{request.FirstName} {request.LastName}", 
                        username, 
                        password
                    );
                    
                    var message = emailResult 
                        ? "Staff member added successfully! Login credentials have been sent to their email."
                        : "Staff member added successfully, but failed to send email. Please provide credentials manually.";
                    
                    return Json(new { success = true, message = message, staff = new { 
                        firstName = request.FirstName,
                        lastName = request.LastName,
                        email = request.Email,
                        username = username
                    }});
                }
                
                return Json(new { success = false, message = "Failed to add staff member. Email may already be in use." });
            }
            catch (Exception ex)
            {
                // Log the actual error for debugging
                Console.WriteLine($"Error in AddStaff: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return Json(new { success = false, message = "An error occurred while adding the staff member." });
            }
        }

        public async Task<IActionResult> History()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var allBookings = await _bookingService.GetAllBookingsAsync();
            var completedBookings = allBookings.Where(b => b.Status == "Returned" || b.Status == "Completed").ToList();
            
            ViewBag.UserRole = userRole;
            return View(completedBookings);
        }
    }
}