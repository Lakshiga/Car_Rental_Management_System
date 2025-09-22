using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Services.Interfaces;
using CarRentalManagementSystem.Enums;

namespace CarRentalManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> RegisterCustomerAsync(CustomerRegistrationRequestDTO request)
        {
            try
            {
                // Check if username or email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);
                
                if (existingUser != null)
                    return false;

                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == request.Email);
                
                if (existingCustomer != null)
                    return false;

                // Create user
                var user = new User
                {
                    Username = request.Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = "Customer"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create customer
                var customer = new Customer
                {
                    UserID = user.UserID,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email
                    // NIC, LicenseNo, PhoneNo, and Address will be collected later through profile update
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool Success, string Token, string Role, int UserId)> LoginAsync(LoginRequestDTO request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                    return (false, string.Empty, string.Empty, 0);

                var token = GenerateJwtToken(user);
                return (true, token, user.Role, user.UserID);
            }
            catch
            {
                return (false, string.Empty, string.Empty, 0);
            }
        }

        public async Task<CustomerResponseDTO?> GetCustomerByUserIdAsync(int userId)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (customer == null)
                return null;

            return new CustomerResponseDTO
            {
                CustomerID = customer.CustomerID,
                FullName = $"{customer.FirstName} {customer.LastName}",
                Email = customer.Email,
                Phone = customer.PhoneNo ?? string.Empty,
                ImageUrl = customer.ImageUrl ?? string.Empty,
                Role = customer.User.Role,
                NIC = customer.NIC ?? string.Empty,
                LicenseNo = customer.LicenseNo ?? string.Empty,
                Address = customer.Address ?? string.Empty
            };
        }

        public async Task<CustomerResponseDTO?> GetCustomerByIdAsync(int customerId)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerID == customerId);

            if (customer == null)
                return null;

            return new CustomerResponseDTO
            {
                CustomerID = customer.CustomerID,
                FullName = $"{customer.FirstName} {customer.LastName}",
                Email = customer.Email,
                Phone = customer.PhoneNo,
                ImageUrl = customer.ImageUrl,
                Role = customer.User.Role,
                NIC = customer.NIC,
                LicenseNo = customer.LicenseNo,
                Address = customer.Address
            };
        }

        public async Task<bool> UpdateCustomerAsync(int customerId, CustomerResponseDTO customerDto)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return false;

                var names = customerDto.FullName.Split(' ', 2);
                customer.FirstName = names[0];
                customer.LastName = names.Length > 1 ? names[1] : "";
                customer.Email = customerDto.Email;
                customer.PhoneNo = customerDto.Phone;
                customer.Address = customerDto.Address;
                customer.NIC = customerDto.NIC;
                customer.LicenseNo = customerDto.LicenseNo;
                customer.ImageUrl = customerDto.ImageUrl;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCustomerProfileAsync(int customerId, CustomerProfileUpdateDTO profileData)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return false;

                // Update only the provided fields
                if (!string.IsNullOrEmpty(profileData.NIC))
                    customer.NIC = profileData.NIC;
                    
                if (!string.IsNullOrEmpty(profileData.Phone))
                    customer.PhoneNo = profileData.Phone;
                    
                if (!string.IsNullOrEmpty(profileData.Address))
                    customer.Address = profileData.Address;
                    
                if (!string.IsNullOrEmpty(profileData.LicenseNo))
                    customer.LicenseNo = profileData.LicenseNo;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<IEnumerable<CustomerResponseDTO>> GetAllCustomersAsync()
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .ToListAsync();

            return customers.Select(customer => new CustomerResponseDTO
            {
                CustomerID = customer.CustomerID,
                FullName = $"{customer.FirstName} {customer.LastName}",
                Email = customer.Email,
                Phone = customer.PhoneNo ?? string.Empty,
                ImageUrl = customer.ImageUrl ?? string.Empty,
                Role = customer.User.Role,
                NIC = customer.NIC ?? string.Empty,
                LicenseNo = customer.LicenseNo ?? string.Empty,
                Address = customer.Address ?? string.Empty
            });
        }

        public async Task<IEnumerable<StaffResponseDTO>> GetAllStaffAsync()
        {
            var staff = await _context.Staff
                .Include(s => s.User)
                .ToListAsync();

            return staff.Select(s => new StaffResponseDTO
            {
                StaffID = s.StaffID,
                UserID = s.UserID,
                FullName = $"{s.FirstName} {s.LastName}",
                Username = s.User.Username,
                Email = s.Email,
                PhoneNo = s.PhoneNumber ?? string.Empty,
                Role = s.User.Role,
                ImageUrl = s.ImageUrl ?? string.Empty,
                IsProfileComplete = s.IsProfileComplete,
                RequirePasswordReset = s.User.RequirePasswordReset,
                CreatedAt = s.User.CreatedAt
            });
        }

        public async Task<bool> RegisterStaffAsync(StaffRegistrationRequestDTO request)
        {
            Console.WriteLine($"RegisterStaffAsync called: FirstName={request.FirstName}, LastName={request.LastName}, Email={request.Email}, Username={request.Username}, Password={request.Password}");
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if email already exists
                var existingStaff = await _context.Staff
                    .FirstOrDefaultAsync(s => s.Email == request.Email);
                
                if (existingStaff != null)
                {
                    Console.WriteLine("Email already exists in database");
                    await transaction.RollbackAsync();
                    return false;
                }

                // Generate credentials if not provided
                string username, password;
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    var credentials = await GenerateStaffCredentialsAsync(request.Email, request.FirstName);
                    username = credentials.Username;
                    password = credentials.Password;
                }
                else
                {
                    username = request.Username;
                    password = request.Password;
                }

                // Check if username already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username);
                
                if (existingUser != null)
                {
                    Console.WriteLine("Username already exists in database");
                    await transaction.RollbackAsync();
                    return false;
                }

                // Create user with password reset requirement
                var user = new User
                {
                    Username = username,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = "Staff",
                    RequirePasswordReset = string.IsNullOrEmpty(request.Password), // Only require reset if password was auto-generated
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create staff
                var staff = new Staff
                {
                    UserID = user.UserID,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    NIC = "", // To be filled later
                    Address = "", // To be filled later
                    IsProfileComplete = false,
                    CreatedAt = DateTime.Now
                };

                _context.Staff.Add(staff);
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();
                Console.WriteLine($"Staff registered successfully: {request.FirstName} {request.LastName} ({request.Email})");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log the error for debugging
                Console.WriteLine($"Error registering staff: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<(string Username, string Password)> GenerateStaffCredentialsAsync(string email, string firstName)
        {
            // Generate username from email prefix and random number
            var emailPrefix = email.Split('@')[0].ToLower();
            var randomSuffix = new Random().Next(100, 999);
            var username = $"{emailPrefix}{randomSuffix}";
            
            // Ensure username is unique
            while (await _context.Users.AnyAsync(u => u.Username == username))
            {
                randomSuffix = new Random().Next(100, 999);
                username = $"{emailPrefix}{randomSuffix}";
            }
            
            // Generate temporary password
            var password = GenerateRandomPassword();
            
            return (username, password);
        }

        public async Task<bool> ResetPasswordAsync(int userId, PasswordResetRequestDTO request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
                    return false;

                // Update password and clear reset requirement
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.RequirePasswordReset = false;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<StaffResponseDTO?> GetStaffByUserIdAsync(int userId)
        {
            var staff = await _context.Staff
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserID == userId);

            if (staff == null)
                return null;

            return new StaffResponseDTO
            {
                StaffID = staff.StaffID,
                UserID = staff.UserID,
                FullName = $"{staff.FirstName} {staff.LastName}",
                Username = staff.User.Username,
                Email = staff.Email,
                PhoneNo = staff.PhoneNumber ?? string.Empty,
                Role = staff.User.Role,
                ImageUrl = staff.ImageUrl ?? string.Empty,
                IsProfileComplete = staff.IsProfileComplete,
                RequirePasswordReset = staff.User.RequirePasswordReset,
                CreatedAt = staff.User.CreatedAt
            };
        }

        public async Task<bool> UpdateStaffProfileAsync(int staffId, StaffResponseDTO staffDto)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff == null)
                    return false;

                var names = staffDto.FullName.Split(' ', 2);
                staff.FirstName = names[0];
                staff.LastName = names.Length > 1 ? names[1] : "";
                staff.Email = staffDto.Email;
                staff.PhoneNumber = staffDto.PhoneNo;
                staff.ImageUrl = staffDto.ImageUrl;
                staff.IsProfileComplete = true; // Mark as complete when updated

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateRandomPassword()
        {
            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
            const string numberChars = "0123456789";
            const string specialChars = "!@#$%^&*";
            
            var random = new Random();
            var password = new char[8];
            
            // Ensure at least one character from each category
            password[0] = upperChars[random.Next(upperChars.Length)];
            password[1] = lowerChars[random.Next(lowerChars.Length)];
            password[2] = numberChars[random.Next(numberChars.Length)];
            password[3] = specialChars[random.Next(specialChars.Length)];
            
            // Fill remaining positions
            var allChars = upperChars + lowerChars + numberChars + specialChars;
            for (int i = 4; i < password.Length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }
            
            // Shuffle the array
            for (int i = password.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }
            
            return new string(password);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(double.Parse(jwtSettings["ExpiryInHours"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}