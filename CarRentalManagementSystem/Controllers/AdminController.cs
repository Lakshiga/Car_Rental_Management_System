using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.Services.Interfaces;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Enums;
using CarRentalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRentalManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ICarService _carService;
        private readonly IBookingService _bookingService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;

        public AdminController(
            ICarService carService,
            IBookingService bookingService,
            IUserService userService,
            IEmailService emailService,
            ApplicationDbContext context)
        {
            _carService = carService;
            _bookingService = bookingService;
            _userService = userService;
            _emailService = emailService;
            _context = context;
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
            ViewBag.PendingBookings = bookings.Count(b => b.Status == "Pending" || b.Status == "Confirmed");            
            ViewBag.UserRole = userRole;

            // Get recent activity data - last 3 bookings
            var recentBookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.RecentBookings = recentBookings;

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
                AllowedKmPerDay = car.AllowedKmPerDay,
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
        public async Task<IActionResult> PreviewBooking(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
                return NotFound();

            ViewBag.UserRole = userRole;
            return View(booking);
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

        [HttpGet]
        public async Task<IActionResult> CustomerDetails(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var customer = await _userService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound();

            return PartialView("~/Views/Admin/_CustomerDetailsPartial.cshtml", customer);
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
        public async Task<IActionResult> ConfirmBooking(int bookingId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Json(new { success = false, message = "Unauthorized" });

            var result = await _bookingService.ConfirmBookingAsync(bookingId);
            
            if (result)
            {
                return Json(new { success = true, message = "Booking confirmed successfully! Customer can now confirm rent." });
            }
            
            return Json(new { success = false, message = "Failed to confirm booking. Booking must be in Pending status." });
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingForRejection(int bookingId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Json(new { success = false, message = "Unauthorized" });

            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
                return Json(new { success = false, message = "Booking not found." });

            var advanceAmount = booking.TotalCost * 0.5m; // 50% advance payment
            
            return Json(new { 
                success = true, 
                bookingId = booking.BookingID,
                customerName = booking.CustomerName,
                carName = booking.CarDetails?.CarName,
                totalCost = booking.TotalCost,
                advanceAmount = advanceAmount
            });
        }

        [HttpPost]
        public async Task<IActionResult> RejectBooking(int bookingId, string rejectionReason)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Json(new { success = false, message = "Unauthorized" });

            if (string.IsNullOrWhiteSpace(rejectionReason))
                return Json(new { success = false, message = "Rejection reason is required." });

            var username = HttpContext.Session.GetString("DisplayName") ?? "Unknown";
            var rejectedByText = $"{userRole} ({username})";
            
            var result = await _bookingService.RejectBookingAsync(bookingId, rejectedByText, rejectionReason);
            
            if (result.Success)
            {
                // Send rejection email with refund instructions
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (booking != null)
                {
                    var customer = await _userService.GetCustomerByUserIdAsync(
                        (await _userService.GetUserByIdAsync(booking.BookingID))?.UserID ?? 0);
                    
                    if (customer != null)
                    {
                        var advanceAmount = booking.TotalCost * 0.5m; // 50% advance payment
                        await _emailService.SendBookingRejectionAsync(customer.Email, customer.FullName, bookingId, rejectionReason, advanceAmount);
                    }
                }
                
                return Json(new { success = true, message = result.Message });
            }
            
            return Json(new { success = false, message = result.Message ?? "Failed to reject booking." });
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
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var staff = await _userService.GetAllStaffAsync();
            ViewBag.UserRole = userRole;
            return View(staff);
        }

        [HttpPost]
        public async Task<IActionResult> AddStaff([FromBody] DTOs.StaffRegistrationRequestDTO request)
        {
            Console.WriteLine($"AddStaff called with data: FirstName={request.FirstName}, LastName={request.LastName}, Email={request.Email}, Username={request.Username}, Password={request.Password}");
            
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                Console.WriteLine("Unauthorized access attempt");
                return Json(new { success = false, message = "Unauthorized" });
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                return Json(new { success = false, message = "First name is required." });
            }
            
            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                return Json(new { success = false, message = "Last name is required." });
            }
            
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Json(new { success = false, message = "Email address is required." });
            }
            
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return Json(new { success = false, message = "Phone number is required." });
            }

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
                Console.WriteLine("Starting staff registration process...");
                
                // Register staff
                var result = await _userService.RegisterStaffAsync(request);
                Console.WriteLine($"Staff registration result: {result}");
                
                if (result)
                {
                    Console.WriteLine("Staff registration successful, generating credentials...");
                    
                    // Get the generated credentials for email
                    string username, password;
                    if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    {
                        Console.WriteLine("Generating auto-credentials...");
                        var credentials = await _userService.GenerateStaffCredentialsAsync(request.Email, request.FirstName);
                        username = credentials.Username;
                        password = credentials.Password;
                        Console.WriteLine($"Generated credentials - Username: {username}");
                    }
                    else
                    {
                        Console.WriteLine("Using provided credentials...");
                        username = request.Username;
                        password = request.Password;
                    }
                    
                    // Send email with credentials
                    Console.WriteLine("Sending credentials email...");
                    var emailResult = await _emailService.SendStaffCredentialsAsync(
                        request.Email, 
                        $"{request.FirstName} {request.LastName}", 
                        username, 
                        password
                    );
                    
                    var message = emailResult 
                        ? "Staff member added successfully! Login credentials have been sent to their email."
                        : "Staff member added successfully, but failed to send email. Please provide credentials manually.";
                    
                    Console.WriteLine($"Staff added successfully: {request.FirstName} {request.LastName} ({request.Email})");
                    Console.WriteLine($"Email sent: {emailResult}");
                    
                    return Json(new { success = true, message = message, staff = new { 
                        firstName = request.FirstName,
                        lastName = request.LastName,
                        email = request.Email,
                        username = username
                    }});
                }
                
                Console.WriteLine($"Failed to add staff member: {request.Email} may already be in use");
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

        [HttpPost]
        public async Task<IActionResult> ConfirmRental(int bookingId, int odometerStart)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Json(new { success = false, message = "Unauthorized" });

            // Get booking details
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null || booking.Status != "Approved")
                return Json(new { success = false, message = "Invalid booking or booking not approved." });

            // Start the rental process
            var result = await _bookingService.StartRentAsync(bookingId, odometerStart);
            
            if (result.Success)
            {
                return Json(new { success = true, message = "Rental confirmed successfully! Booking status changed to Rented." });
            }
            
            return Json(new { success = false, message = "Failed to confirm rental. Please try again." });
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
        
        [HttpGet]
        public async Task<IActionResult> PreviewBookingPartial(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Unauthorized();

            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
                return NotFound();

            return PartialView("~/Views/Admin/PreviewBooking.cshtml", booking);
        }

        // Add the missing staff management methods here
        [HttpGet]
        public async Task<IActionResult> GetStaff(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                // Get staff by staff ID, not user ID
                var staff = await _context.Staff
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.StaffID == id);
                    
                if (staff == null)
                    return Json(new { success = false, message = "Staff member not found." });

                // Return flattened object to match frontend expectations
                return Json(new { 
                    staffID = staff.StaffID,
                    firstName = staff.FirstName,
                    lastName = staff.LastName,
                    email = staff.Email,
                    phoneNo = staff.PhoneNumber ?? ""
                });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to retrieve staff details." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStaff([FromBody] DTOs.StaffResponseDTO request)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data provided." });
            }

            try
            {
                var result = await _userService.UpdateStaffProfileAsync(request.StaffID, request);
                
                if (result)
                {
                    return Json(new { success = true, message = "Updated Successfully" });
                }
                
                return Json(new { success = false, message = "Failed to update staff member." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the staff member." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                // First get the staff member to ensure they exist
                var staff = await _context.Staff
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.StaffID == id);

                if (staff == null)
                    return Json(new { success = false, message = "Staff member not found." });

                // Prevent deletion of admin users
                if (staff.User.Role == "Admin")
                    return Json(new { success = false, message = "Cannot delete admin users." });

                // Delete the user and staff records
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Users.Remove(staff.User);
                    _context.Staff.Remove(staff);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Staff member deleted successfully!" });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Failed to delete staff member." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting the staff member." });
            }
        }
    }
}