using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Services.Interfaces;

namespace CarRentalManagementSystem.Controllers
{
    public class RentController : Controller
    {
        private readonly IRentService _rentService;
        private readonly IBookingService _bookingService;

        public RentController(IRentService rentService, IBookingService bookingService)
        {
            _rentService = rentService;
            _bookingService = bookingService;
        }

        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var rents = await _rentService.GetAllRentsAsync();
            ViewBag.UserRole = userRole;
            return View(rents);
        }

        public async Task<IActionResult> ActiveRents()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var activeRents = await _rentService.GetActiveRentsAsync();
            ViewBag.UserRole = userRole;
            return View(activeRents);
        }

        [HttpGet]
        public async Task<IActionResult> StartRent(int bookingId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null || booking.Status != "Approved")
            {
                TempData["ErrorMessage"] = "Invalid booking or booking not approved.";
                return RedirectToAction("Bookings", "Admin");
            }

            ViewBag.Booking = booking;
            return View(new StartRentRequestDTO { BookingID = bookingId });
        }

        [HttpPost]
        public async Task<IActionResult> StartRent(StartRentRequestDTO request)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
            {
                var booking = await _bookingService.GetBookingByIdAsync(request.BookingID);
                ViewBag.Booking = booking;
                return View(request);
            }

            var result = await _rentService.StartRentAsync(request.BookingID, request.OdometerStart);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Rent started successfully! Car has been marked as rented.";
                return RedirectToAction("ActiveRents");
            }
            
            TempData["ErrorMessage"] = "Failed to start rent. Please check the booking status.";
            return RedirectToAction("Bookings", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> ProcessReturn(int rentId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var rent = await _rentService.GetRentByIdAsync(rentId);
            if (rent == null || rent.IsReturned)
            {
                TempData["ErrorMessage"] = "Invalid rent or car already returned.";
                return RedirectToAction("ActiveRents");
            }

            ViewBag.Rent = rent;
            return View(new ProcessReturnRequestDTO { RentID = rentId });
        }

        [HttpPost]
        public async Task<IActionResult> ProcessReturn(ProcessReturnRequestDTO request)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
            {
                var rent = await _rentService.GetRentByIdAsync(request.RentID);
                ViewBag.Rent = rent;
                return View(request);
            }

            var result = await _rentService.ProcessReturnAsync(request.RentID, request.OdometerEnd, request.ActualReturnDate);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Car returned successfully! Extra charges have been calculated.";
                return RedirectToAction("Index");
            }
            
            TempData["ErrorMessage"] = "Failed to process return. Please try again.";
            return RedirectToAction("ActiveRents");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var rent = await _rentService.GetRentByIdAsync(id);
            if (rent == null)
                return NotFound();

            return View(rent);
        }

        [HttpGet]
        public async Task<IActionResult> CalculateExtraCharges(int rentId, int odometerEnd)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Json(new { success = false, message = "Unauthorized" });

            var extraCharges = await _rentService.CalculateExtraChargesAsync(rentId, odometerEnd);
            return Json(new { success = true, extraCharges = extraCharges });
        }

        public async Task<IActionResult> PendingPayments()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return RedirectToAction("Login", "Account");

            var pendingPayments = await _rentService.GetPendingPaymentsAsync();
            ViewBag.UserRole = userRole;
            return View(pendingPayments);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessFinalPayment(int returnId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin" && userRole != "Staff")
                return Json(new { success = false, message = "Unauthorized" });

            var result = await _rentService.ProcessFinalPaymentAsync(returnId);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Final payment processed successfully! Rental completed.";
                return Json(new { success = true, message = "Final payment processed successfully!" });
            }
            
            return Json(new { success = false, message = "Failed to process final payment." });
        }
    }
}