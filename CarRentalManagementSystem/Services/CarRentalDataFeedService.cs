using CarRentalManagementSystem.Services.Interfaces;
using CarRentalManagementSystem.DTOs;

namespace CarRentalManagementSystem.Services
{
    public class CarRentalDataFeedService
    {
        private readonly IBookingService _bookingService;
        private readonly ICarService _carService;
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;
        private readonly IRentService _rentService;
        private readonly ILogger<CarRentalDataFeedService> _logger;

        public CarRentalDataFeedService(
            IBookingService bookingService,
            ICarService carService,
            IUserService userService,
            IPaymentService paymentService,
            IRentService rentService,
            ILogger<CarRentalDataFeedService> logger)
        {
            _bookingService = bookingService;
            _carService = carService;
            _userService = userService;
            _paymentService = paymentService;
            _rentService = rentService;
            _logger = logger;
        }

        public async Task<string> GetCarRentalKnowledgeBaseAsync()
        {
            var knowledgeBase = @"
# Car Rental Management System Knowledge Base

## System Overview
This is a comprehensive car rental management system that allows customers to browse, book, and manage car rentals online while providing administrators with tools to manage the entire rental operation.

## Customer Features & Processes

### 1. Account Management
- **Registration**: Customers can create accounts with email and personal information
- **Login**: Secure authentication system
- **Profile Management**: Update personal information, upload profile pictures
- **Password Reset**: Email-based password recovery system

### 2. Car Browsing & Booking
- **Browse Cars**: View available cars with details (brand, model, year, daily rate)
- **Car Search**: Filter cars by brand, availability, and price range
- **Booking Process**: 
  - Select dates (start and end)
  - Choose car
  - Provide booking details
  - Submit for admin approval
- **Booking Status**: Pending → Approved → Rented → Returned

### 3. Payment System
- **Advance Payment**: 50% payment required upon booking approval
- **Payment Methods**: Credit card processing via Stripe
- **Payment Tracking**: View payment history and status
- **Settlement**: Final payment processing upon return

### 4. Customer Dashboard
- **Booking Overview**: View all bookings (past and current)
- **Active Rentals**: Track ongoing rentals
- **Payment History**: Review all payments made
- **Quick Actions**: Make new bookings, view available cars

## Admin/Staff Features & Processes

### 1. Booking Management
- **Approval System**: Review and approve/reject customer bookings
- **Booking Overview**: View all bookings with filtering options
- **Status Management**: Update booking statuses
- **Communication**: Send automated emails for approvals/rejections

### 2. Car Management
- **Car Inventory**: Add, edit, and delete cars from fleet
- **Car Status**: Available, Rented, Maintenance, Out of Service
- **Car Details**: Manage specifications, images, and pricing
- **Odometer Tracking**: Record start and end odometer readings

### 3. Customer Management
- **Customer Profiles**: View customer information and history
- **Booking History**: Track customer rental patterns
- **Communication**: Send notifications and updates

### 4. Rental Operations
- **Rental Confirmation**: Process approved bookings into active rentals
- **Return Processing**: Handle car returns and damage assessment
- **Settlement**: Process final payments and additional charges
- **Damage Assessment**: Record any damages or issues

### 5. Staff Management (Admin only)
- **Staff Registration**: Add new staff members
- **Role Management**: Assign permissions and roles
- **Credential Management**: Generate and send login credentials

### 6. Payment Processing
- **Payment Oversight**: Monitor all transactions
- **Refund Processing**: Handle refunds when necessary
- **Financial Reporting**: Track revenue and payment statuses

### 7. Admin Dashboard
- **System Overview**: Key metrics and statistics
- **Recent Activity**: Latest bookings, payments, returns
- **Pending Actions**: Items requiring attention
- **Reports**: Generate various business reports

## Business Rules & Policies

### Booking Rules
- Advance payment of 50% required for all bookings
- Admin approval required before rental confirmation
- Minimum rental period: 1 day
- Maximum advance booking: Based on car availability

### Payment Policies
- Payment methods: Credit/Debit cards via Stripe
- Refund policy applies for cancelled bookings
- Additional charges for damages or extra kilometers
- Late return fees may apply

### Car Availability
- Cars marked as 'Available' can be booked
- Regular maintenance scheduling affects availability
- Damaged cars are marked 'Out of Service' until repaired

### User Roles & Permissions
- **Customer**: Book cars, make payments, manage profile
- **Staff**: Manage bookings, cars, process returns (limited access)
- **Admin**: Full system access including staff management

## System Integrations

### Email Service
- Booking confirmations and updates
- Staff credential delivery
- Password reset notifications
- Payment confirmations

### Payment Gateway
- Stripe integration for secure payments
- Real-time payment processing
- Payment status tracking

### Database Management
- SQL Server database
- Entity Framework for data access
- Automated migrations

## Common Procedures

### For Customers:
1. **Making a Booking**:
   - Browse available cars
   - Select dates and car
   - Fill booking form
   - Wait for admin approval
   - Make advance payment when approved
   - Receive confirmation

2. **Managing Bookings**:
   - View booking status on dashboard
   - Update booking if allowed
   - Make payments when due
   - Track rental progress

### For Admin/Staff:
1. **Processing Bookings**:
   - Review new booking requests
   - Verify customer information
   - Check car availability
   - Approve or reject with reasons
   - Send confirmation emails

2. **Managing Rentals**:
   - Confirm rental start with odometer reading
   - Monitor active rentals
   - Process returns with damage assessment
   - Handle final settlements

This knowledge base covers the core functionality and processes of the car rental management system. Always direct users to the appropriate features based on their role and current needs.
";
            return knowledgeBase;
        }

        public async Task<Dictionary<string, object>> GetRealTimeDataAsync(string userType, string? userId = null)
        {
            var data = new Dictionary<string, object>();

            try
            {
                // Common data for all users
                var cars = await _carService.GetAllCarsAsync();
                data["TotalCars"] = cars.Count();
                data["AvailableCars"] = cars.Count(c => c.AvailabilityStatus == "Available");
                data["RentedCars"] = cars.Count(c => c.AvailabilityStatus == "Rented");

                var allBookings = await _bookingService.GetAllBookingsAsync();
                data["TotalBookings"] = allBookings.Count();
                data["PendingBookings"] = allBookings.Count(b => b.Status == "Pending");
                data["ApprovedBookings"] = allBookings.Count(b => b.Status == "Approved");

                // User-specific data
                if (userType.ToLower() == "customer" && !string.IsNullOrEmpty(userId))
                {
                    if (int.TryParse(userId, out int customerId))
                    {
                        var customerBookings = await _bookingService.GetBookingsByCustomerAsync(customerId);
                        data["CustomerBookings"] = customerBookings.Count();
                        data["CustomerActiveBookings"] = customerBookings.Count(b => 
                            b.Status == "Approved" || b.Status == "Confirmed" || b.Status == "Rented");
                    }
                }

                // Popular car brands
                var popularBrands = cars.GroupBy(c => c.Brand)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => new { Brand = g.Key, Count = g.Count() })
                    .ToList();
                data["PopularBrands"] = popularBrands;

                // Recent activity (last 7 days)
                var recentBookings = allBookings.Where(b => b.CreatedAt >= DateTime.Now.AddDays(-7))
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .Select(b => new { 
                        Car = b.CarDetails.CarName, 
                        Customer = b.CustomerName, 
                        Date = b.CreatedAt.ToString("yyyy-MM-dd"),
                        Status = b.Status 
                    })
                    .ToList();
                data["RecentBookings"] = recentBookings;

                // System health indicators
                data["SystemHealth"] = new
                {
                    Status = "Operational",
                    LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ActiveUsers = "Multiple", // Could be enhanced with real user tracking
                    DatabaseStatus = "Connected"
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving real-time data");
                data["Error"] = "Some data temporarily unavailable";
            }

            return data;
        }

        public async Task<List<string>> GetFrequentlyAskedQuestionsAsync(string userType)
        {
            var faqs = new List<string>();

            if (userType.ToLower() == "customer")
            {
                faqs.AddRange(new[]
                {
                    "How do I book a car?",
                    "What documents do I need for rental?",
                    "How much advance payment is required?",
                    "Can I modify my booking?",
                    "What happens if I return the car late?",
                    "How do I make a payment?",
                    "What is included in the rental price?",
                    "Can I cancel my booking?",
                    "How do I check my booking status?",
                    "What if the car breaks down during my rental?"
                });
            }
            else
            {
                faqs.AddRange(new[]
                {
                    "How do I approve a booking?",
                    "How do I add a new car to the fleet?",
                    "How do I process a car return?",
                    "How do I handle payment issues?",
                    "How do I generate reports?",
                    "How do I manage staff accounts?",
                    "How do I handle customer complaints?",
                    "How do I update car availability?",
                    "How do I process damage claims?",
                    "How do I view system statistics?"
                });
            }

            return faqs;
        }

        public async Task<string> GetContextualHelpAsync(string userType, string currentPage)
        {
            var helpContent = userType.ToLower() == "customer" 
                ? GetCustomerHelpContent(currentPage)
                : GetAdminHelpContent(currentPage);

            return helpContent;
        }

        private string GetCustomerHelpContent(string currentPage)
        {
            return currentPage.ToLower() switch
            {
                "dashboard" => "Welcome to your dashboard! Here you can view your bookings, make new reservations, and track your rental history. Use the navigation menu to access different features.",
                "booking" => "To book a car, select your desired dates, choose from available cars, and fill in the booking details. Your booking will need admin approval before confirmation.",
                "payment" => "Make secure payments using your credit or debit card. Advance payment (50%) is required upon booking approval, with final settlement upon return.",
                "profile" => "Keep your profile information up to date for smooth rental processes. You can update your details and upload a profile picture here.",
                _ => "Welcome to our car rental system! I can help you with bookings, payments, car availability, and general rental information. What would you like to know?"
            };
        }

        private string GetAdminHelpContent(string currentPage)
        {
            return currentPage.ToLower() switch
            {
                "dashboard" => "Admin dashboard provides system overview including pending bookings, car availability, and recent activities. Use the sidebar to navigate to different management sections.",
                "bookings" => "Manage all customer bookings here. You can approve or reject pending bookings, view booking details, and track rental status.",
                "cars" => "Car management allows you to add, edit, and remove cars from your fleet. Update availability status and manage car details.",
                "customers" => "View and manage customer information, booking history, and handle customer-related inquiries.",
                "payments" => "Monitor all payment transactions, process refunds, and handle payment-related issues.",
                "staff" => "Staff management (Admin only) - Add new staff members, manage roles, and handle staff credentials.",
                _ => "Welcome to the admin panel! I can help you with booking management, car fleet operations, customer management, payment processing, and system administration. What do you need assistance with?"
            };
        }
    }
}