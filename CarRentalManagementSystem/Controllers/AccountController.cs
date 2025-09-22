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
                else if (result.Role == "Staff")
                {
                    var staff = await _userService.GetStaffByUserIdAsync(result.UserId);
                    if (staff != null)
                    {
                        HttpContext.Session.SetString("StaffId", staff.StaffID.ToString());
                        HttpContext.Session.SetString("StaffName", staff.FullName);
                        if (!string.IsNullOrWhiteSpace(staff.ImageUrl))
                        {
                            HttpContext.Session.SetString("AvatarUrl", staff.ImageUrl);
                        }
                        
                        // Check if password reset is required
                        if (staff.RequirePasswordReset)
                        {
                            return RedirectToAction("ResetPassword");
                        }
                        
                        // Check if profile is complete
                        if (!staff.IsProfileComplete)
                        {
                            TempData["WarningMessage"] = "Please complete your profile before accessing system features.";
                            return RedirectToAction("CompleteProfile");
                        }
                    }
                }

                // Default display name for non-customer
                if (result.Role != "Customer")
                {
                    var displayName = result.Role == "Staff" ? 
                        HttpContext.Session.GetString("StaffName") ?? model.Username :
                        model.Username;
                    HttpContext.Session.SetString("DisplayName", displayName);
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
            // Add a flag to indicate logout happened so frontend can clear chat histories
            TempData["ChatCleared"] = "true";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> GetMyLicense()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || userRole != "Customer")
                return Json(new { success = false, message = "Not logged in as customer" });

            var userId = int.Parse(userIdString);
            var customer = await _userService.GetCustomerByUserIdAsync(userId);
            var license = customer?.LicenseNo ?? string.Empty;
            return Json(new { success = true, licenseNo = license });
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

        [HttpGet]
        public async Task<IActionResult> ResetPassword()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userIdString) || userRole != "Staff")
                return RedirectToAction("Login");
                
            var userId = int.Parse(userIdString);
            var staff = await _userService.GetStaffByUserIdAsync(userId);
            
            if (staff == null || !staff.RequirePasswordReset)
                return RedirectToAction("Dashboard", "Admin");
                
            return View(new PasswordResetRequestDTO());
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(PasswordResetRequestDTO model)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userIdString) || userRole != "Staff")
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var userId = int.Parse(userIdString);
            var result = await _userService.ResetPasswordAsync(userId, model);
            
            if (result)
            {
                // Show success then take user to Login so they can authenticate with the NEW password
                TempData["SuccessMessage"] = "Password reset successfully! Please login with your new password.";
                return RedirectToAction("Login");
            }
            
            ModelState.AddModelError("", "Failed to reset password. Please check your current password.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CompleteProfile()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userIdString) || userRole != "Staff")
                return RedirectToAction("Login");
                
            var userId = int.Parse(userIdString);
            var staff = await _userService.GetStaffByUserIdAsync(userId);
            
            if (staff == null)
                return RedirectToAction("Login");
                
            if (staff.IsProfileComplete)
                return RedirectToAction("Dashboard", "Admin");
                
            return View(staff);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteProfile(StaffResponseDTO model, IFormFile? imageFile)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var staffIdString = HttpContext.Session.GetString("StaffId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(staffIdString) || userRole != "Staff")
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var staffId = int.Parse(staffIdString);
            
            // Handle image upload
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

            var result = await _userService.UpdateStaffProfileAsync(staffId, model);
            
            if (result)
            {
                HttpContext.Session.SetString("StaffName", model.FullName);
                HttpContext.Session.SetString("DisplayName", model.FullName);
                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    HttpContext.Session.SetString("AvatarUrl", model.ImageUrl);
                }
                
                TempData["SuccessMessage"] = "Profile completed successfully! Welcome to the system.";
                return RedirectToAction("Dashboard", "Admin");
            }
            
            ModelState.AddModelError("", "Failed to update profile.");
            return View(model);
        }
    }
}