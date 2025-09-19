using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Services.Interfaces;

namespace CarRentalManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");
                
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(CustomerRegistrationRequestDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userService.RegisterCustomerAsync(model);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Registration successful! Please login to continue.";
                return RedirectToAction("Login");
            }
            
            ModelState.AddModelError("", "Registration failed. Username or email might already exist.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");
                
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userService.LoginAsync(model);
            
            if (result.Success)
            {
                HttpContext.Session.SetString("UserId", result.UserId.ToString());
                HttpContext.Session.SetString("UserRole", result.Role);
                HttpContext.Session.SetString("Token", result.Token);

                // Get customer details if it's a customer
                if (result.Role == "Customer")
                {
                    var customer = await _userService.GetCustomerByUserIdAsync(result.UserId);
                    if (customer != null)
                    {
                        HttpContext.Session.SetString("CustomerId", customer.CustomerID.ToString());
                        HttpContext.Session.SetString("CustomerName", customer.FullName);
                    }
                }

                // Redirect based on role
                return result.Role switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Admin"),
                    "Staff" => RedirectToAction("Dashboard", "Admin"), // Staff uses same dashboard
                    "Customer" => RedirectToAction("Dashboard", "Customer"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
            
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login");

            var userId = int.Parse(userIdString);
            var customer = await _userService.GetCustomerByUserIdAsync(userId);
            
            if (customer == null)
                return RedirectToAction("Login");

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(CustomerResponseDTO model)
        {
            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString))
                return RedirectToAction("Login");

            var customerId = int.Parse(customerIdString);
            var result = await _userService.UpdateCustomerAsync(customerId, model);
            
            if (result)
            {
                HttpContext.Session.SetString("CustomerName", model.FullName);
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update profile.";
            }

            return View(model);
        }
    }
}