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
                customer.ImageUrl = customerDto.ImageUrl;

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