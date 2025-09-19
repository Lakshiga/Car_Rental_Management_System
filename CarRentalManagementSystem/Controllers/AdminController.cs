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

            var result = await _carService.AddCarAsync(model);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Car added successfully!";
                return RedirectToAction("Cars");
            }
            
            ModelState.AddModelError("", "Failed to add car.");
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

        [HttpPost]
        public async Task<IActionResult> ApproveBooking(int bookingId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Json(new { success = false, message = "Unauthorized" });

            var result = await _bookingService.ApproveBookingAsync(bookingId);
            
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

            var result = await _bookingService.RejectBookingAsync(bookingId);
            
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
    }
}