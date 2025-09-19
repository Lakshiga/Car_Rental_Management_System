using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.Services.Interfaces;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Data;
using System.Diagnostics;

namespace CarRentalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICarService _carService;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ICarService carService, IEmailService emailService, ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _carService = carService;
            _emailService = emailService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var cars = await _carService.GetAvailableCarsAsync();
            ViewBag.IsLoggedIn = HttpContext.Session.GetString("UserId") != null;
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            ViewBag.CustomerName = HttpContext.Session.GetString("CustomerName");
            
            return View(cars);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitContact(Contact contact)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    contact.CreatedAt = DateTime.Now;
                    _context.Contacts.Add(contact);
                    await _context.SaveChangesAsync();

                    // Send acknowledgment email
                    await _emailService.SendContactAcknowledgmentAsync(contact.Email, contact.Name);

                    return Json(new { success = true, message = "Thank you for your message. We'll get back to you soon!" });
                }
                return Json(new { success = false, message = "Please fill in all required fields." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing contact form");
                return Json(new { success = false, message = "An error occurred. Please try again." });
            }
        }

        public IActionResult Gallery()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        public async Task<IActionResult> BrowseCars(string searchTerm, string carType, string fuelType, string seatingCapacity, string priceRange)
        {
            var cars = await _carService.SearchCarsAsync(searchTerm, carType, fuelType);
            
            // Additional filtering for seating capacity
            if (!string.IsNullOrEmpty(seatingCapacity) && int.TryParse(seatingCapacity, out int seats))
            {
                cars = cars.Where(c => c.SeatingCapacity >= seats);
            }
            
            // Additional filtering for price range
            if (!string.IsNullOrEmpty(priceRange))
            {
                var range = priceRange.Split('-');
                if (range.Length == 2 && decimal.TryParse(range[0], out decimal minPrice) && decimal.TryParse(range[1], out decimal maxPrice))
                {
                    cars = cars.Where(c => c.RentPerDay >= minPrice && c.RentPerDay <= maxPrice);
                }
            }
            
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CarType = carType;
            ViewBag.FuelType = fuelType;
            ViewBag.SeatingCapacity = seatingCapacity;
            ViewBag.PriceRange = priceRange;
            ViewBag.IsLoggedIn = HttpContext.Session.GetString("UserId") != null;
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            ViewBag.CustomerName = HttpContext.Session.GetString("CustomerName");
            
            return View(cars);
        }

        [HttpGet]
        public async Task<IActionResult> GetCarDetails(int id)
        {
            var car = await _carService.GetCarByIdAsync(id);
            if (car == null)
                return NotFound();

            return Json(car);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}