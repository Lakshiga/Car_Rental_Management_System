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
                HttpContext.Session.SetString("Username", model.Username);

                // Get customer details if it's a customer
                if (result.Role == "Customer")
                {
                    var customer = await _userService.GetCustomerByUserIdAsync(result.UserId);
                    if (customer != null)
                    {
                        HttpContext.Session.SetString("CustomerId", customer.CustomerID.ToString());
                        HttpContext.Session.SetString("CustomerName", customer.FullName);
                        if (!string.IsNullOrWhiteSpace(customer.ImageUrl))
                        {
                            HttpContext.Session.SetString("AvatarUrl", customer.ImageUrl);
                        }
                    }
                }

                // Default display name for non-customer
                if (result.Role != "Customer")
                {
                    HttpContext.Session.SetString("DisplayName", model.Username);
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
            var userRole = HttpContext.Session.GetString("UserRole");
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login");

            var userId = int.Parse(userIdString);

            if (userRole == "Admin" || userRole == "Staff")
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                    return RedirectToAction("Login");

                var adminModel = new CustomerResponseDTO
                {
                    CustomerID = 0,
                    FullName = user.Username,
                    Email = string.Empty,
                    Phone = string.Empty,
                    Role = userRole ?? string.Empty,
                    NIC = string.Empty,
                        LicenseNo = string.Empty,
                        Address = string.Empty,
                        ImageUrl = HttpContext.Session.GetString("AvatarUrl") ?? string.Empty
                };

                return View(adminModel);
            }

            var customer = await _userService.GetCustomerByUserIdAsync(userId);
            if (customer == null)
                return RedirectToAction("Login");

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(CustomerResponseDTO model, IFormFile? imageFile)
        {
            var customerIdString = HttpContext.Session.GetString("CustomerId");
            var userRole = HttpContext.Session.GetString("UserRole");

            // Admin/Staff can update their basic profile (username only for now)
            if (userRole == "Admin" || userRole == "Staff")
            {
                var userIdString = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                    return RedirectToAction("Login");

                // Handle avatar upload to wwwroot/images/profiles
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

                HttpContext.Session.SetString("DisplayName", model.FullName);
                TempData["SuccessMessage"] = "Profile updated.";
                return View(model);
            }

            if (string.IsNullOrEmpty(customerIdString))
                return RedirectToAction("Login");

            var customerId = int.Parse(customerIdString);

            // handle image upload for customer
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
                model.ImageUrl = "/images/profiles/" + uniqueFileName;
            }

            var result = await _userService.UpdateCustomerAsync(customerId, model);
            
            if (result)
            {
                HttpContext.Session.SetString("CustomerName", model.FullName);
                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    HttpContext.Session.SetString("AvatarUrl", model.ImageUrl);
                }
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