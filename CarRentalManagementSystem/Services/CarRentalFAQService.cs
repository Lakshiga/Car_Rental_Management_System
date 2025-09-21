using CarRentalManagementSystem.Services.Interfaces;

namespace CarRentalManagementSystem.Services
{
    public class CarRentalFAQService
    {
        private readonly ILogger<CarRentalFAQService> _logger;

        public CarRentalFAQService(ILogger<CarRentalFAQService> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetEnhancedKnowledgeBaseAsync()
        {
            var knowledgeBase = @"
# Enhanced Car Rental Management System Knowledge Base

## User Flow Guides

### CUSTOMER FLOW (Quick Steps)
1. **Register/Login** → Create account or sign in
2. **Browse Cars** → View available vehicles with details
3. **Select & Book** → Choose car, dates, upload license
4. **Wait for Approval** → Admin reviews and approves booking
5. **Make Payment** → Pay 50% advance via credit card
6. **Get Confirmation** → Receive booking confirmation email
7. **Pick Up Car** → Collect vehicle with odometer reading
8. **Return Car** → Return vehicle and complete settlement

### ADMIN FLOW (Management Steps)
1. **Dashboard Overview** → View system statistics and metrics
2. **Review Bookings** → Approve/reject customer requests
3. **Manage Fleet** → Add/edit/remove cars from inventory
4. **Staff Management** → Add/manage staff accounts
5. **Customer Management** → View customer profiles and history
6. **Process Rentals** → Confirm rentals and handle returns
7. **Payment Oversight** → Monitor transactions and settlements
8. **Generate Reports** → Business analytics and insights

### STAFF FLOW (Operational Steps)
1. **Dashboard Review** → Check pending tasks and operations
2. **Process Bookings** → Approve/reject customer requests
3. **Rental Operations** → Confirm rentals and odometer readings
4. **Handle Returns** → Process vehicle returns and damage assessment
5. **Customer Service** → Assist customers with inquiries
6. **Settlement Processing** → Complete final payments and charges
7. **Status Updates** → Update booking and rental statuses
8. **Documentation** → Maintain rental records and receipts

## Project-Specific FAQ Responses

### Core Rental Questions & Answers

**Q: What documents are required to rent a car?**
A: You need a valid driving license, ID proof, and in some cases a credit card.

**Q: Do you allow one-way rentals?**
A: Yes, one-way rentals are available but may include extra charges.

**Q: Is insurance included in the rental cost?**
A: Basic insurance is included, but you can purchase additional coverage.

**Q: What happens if I return the car late?**
A: Late returns may incur additional hourly or daily charges.

**Q: Do you provide child seats?**
A: Yes, child seats are available on request for an extra fee.

**Q: Are pets allowed in rental cars?**
A: Yes, pets are allowed, but cleaning fees may apply.

**Q: Do you offer long-term rentals?**
A: Yes, we offer weekly and monthly rental plans with discounts.

**Q: Is fuel included in the rental price?**
A: No, fuel is not included. Please return the car with the same fuel level.

**Q: Do you provide roadside assistance?**
A: Yes, 24/7 roadside assistance is included in all rentals.

**Q: Can I rent a car without a credit card?**
A: Some cars can be rented with a debit card and deposit, but most require a credit card.

## Detailed Booking Process

### Step-by-Step Customer Booking Process:
1. **Browse Available Cars**: Use our car listing page to view all available vehicles
2. **Select Your Car**: Choose based on your needs, budget, and preferences
3. **Choose Dates**: Select pickup and return dates using our calendar system
4. **Fill Booking Form**: Provide personal details, contact information
5. **Upload Documents**: Upload front and back images of your driving license
6. **Submit Request**: Send booking request for admin review
7. **Wait for Approval**: Admin will verify your documents and approve/reject
8. **Make Payment**: Pay 50% advance payment via Stripe when approved
9. **Receive Confirmation**: Get booking confirmation with pickup details
10. **Car Pickup**: Collect your car with odometer reading recorded
11. **Enjoy Your Rental**: Use the vehicle according to rental terms
12. **Return Process**: Return car with final odometer reading and condition check
13. **Final Settlement**: Complete any remaining payments or damage charges

### Admin Approval Process:
1. **Review Request**: Admin receives new booking notification
2. **Verify Documents**: Check uploaded license images for validity
3. **Check Availability**: Confirm car availability for requested dates
4. **Customer Verification**: Review customer information and history
5. **Decision**: Approve or reject with detailed reasoning
6. **Notification**: Automated email sent to customer with decision
7. **Payment Trigger**: If approved, customer can proceed with payment

## System Features by User Role

### Customer Portal Features:
- **Dashboard Overview**: Personal booking summary and quick stats
- **Car Browsing**: Advanced search and filter options
- **Booking Management**: Create, view, and track rental requests
- **Payment Processing**: Secure online payment via Stripe
- **Document Upload**: License image upload with validation
- **Profile Management**: Update personal information and preferences
- **History Tracking**: Complete rental and payment history
- **Support Access**: Direct communication with support team

### Admin/Staff Portal Features:
- **Booking Oversight**: Comprehensive booking management dashboard
- **Customer Management**: Customer profiles and rental history
- **Fleet Management**: Add, edit, remove vehicles from system
- **Approval Workflow**: Streamlined booking approval process
- **Payment Monitoring**: Track all payments and financial metrics
- **Staff Administration**: Manage staff accounts and permissions (Admin only)
- **Return Processing**: Handle vehicle returns and damage assessment
- **Reporting Tools**: Generate business reports and analytics
- **System Configuration**: Manage rates, policies, and settings

## Technical Implementation Details

### Payment Integration:
- **Stripe Gateway**: Secure credit card processing
- **Advance Payment**: 50% required upon booking approval
- **Payment Tracking**: Real-time status updates
- **Multiple Methods**: Support for various card types
- **Automatic Receipts**: Email confirmations for all transactions

### Document Management:
- **Image Upload**: Secure license image storage
- **Verification Process**: Admin review of uploaded documents
- **File Validation**: Automatic image format and size checks
- **Secure Storage**: Encrypted document storage system

### Communication System:
- **Email Notifications**: Automated status updates
- **SMS Integration**: Optional text message alerts
- **In-App Messaging**: Direct communication channels
- **Status Tracking**: Real-time booking status updates

### Security Features:
- **User Authentication**: Secure login with session management
- **Role-Based Access**: Different permissions for each user type
- **Data Encryption**: Secure handling of sensitive information
- **Audit Trail**: Complete activity logging for accountability

## Operational Policies

### Pricing Structure:
- **Daily Rates**: Fixed per-day pricing for each vehicle
- **Per-Kilometer Charges**: Additional fees for excess mileage
- **Advance Payment**: 50% required upon approval
- **Late Fees**: Calculated based on daily rates
- **Damage Charges**: Assessed during return process

### Vehicle Management:
- **Availability Tracking**: Real-time availability updates
- **Maintenance Scheduling**: Regular service and inspection
- **Condition Monitoring**: Pre and post-rental inspections
- **Odometer Management**: Accurate mileage tracking
- **Fleet Optimization**: Strategic vehicle allocation

### Customer Service:
- **24/7 Support**: Round-the-clock assistance
- **Multi-Channel Support**: Phone, email, and in-app help
- **Issue Resolution**: Structured problem-solving process
- **Feedback System**: Customer satisfaction tracking
- **Quality Assurance**: Continuous service improvement

This enhanced knowledge base provides comprehensive coverage of all system features, processes, and policies to ensure accurate and helpful responses to customer and admin inquiries using the exact project flow answers.
";
            return await Task.FromResult(knowledgeBase);
        }

        public Dictionary<string, string> GetFrequentQuestions()
        {
            return new Dictionary<string, string>
            {
                // Core FAQ Questions with Project-Specific Answers
                ["What documents are required to rent a car?"] = "You need a valid driving license, ID proof, and in some cases a credit card.",
                ["Do you allow one-way rentals?"] = "Yes, one-way rentals are available but may include extra charges.",
                ["Is insurance included in the rental cost?"] = "Basic insurance is included, but you can purchase additional coverage.",
                ["What happens if I return the car late?"] = "Late returns may incur additional hourly or daily charges.",
                ["Do you provide child seats?"] = "Yes, child seats are available on request for an extra fee.",
                ["Are pets allowed in rental cars?"] = "Yes, pets are allowed, but cleaning fees may apply.",
                ["Do you offer long-term rentals?"] = "Yes, we offer weekly and monthly rental plans with discounts.",
                ["Is fuel included in the rental price?"] = "No, fuel is not included. Please return the car with the same fuel level.",
                ["Do you provide roadside assistance?"] = "Yes, 24/7 roadside assistance is included in all rentals.",
                ["Can I rent a car without a credit card?"] = "Some cars can be rented with a debit card and deposit, but most require a credit card.",
                
                // Additional Booking Process Questions
                ["How do I book a car?"] = "Browse available cars, select your preferred vehicle and dates, fill out the booking form with your details, upload your license images, submit for admin approval, make 50% advance payment when approved, and receive confirmation.",
                ["How long does approval take?"] = "Booking approvals typically take 2-4 hours during business hours. You'll receive an email notification once your booking is reviewed and approved or if any additional information is needed.",
                ["Can I modify my booking?"] = "Booking modifications depend on the current status. Pending bookings can often be modified. Contact our support team for assistance with approved bookings.",
                ["What if my booking is rejected?"] = "If your booking is rejected, you'll receive an email with the specific reason. Common reasons include invalid documents, car unavailability, or incomplete information. You can resubmit after addressing the issues.",
                
                // Payment Questions
                ["How much advance payment is required?"] = "50% advance payment is required upon booking approval. The remaining balance is settled during vehicle return along with any additional charges.",
                ["What payment methods do you accept?"] = "We accept all major credit and debit cards through our secure Stripe payment gateway. Cash payments are not accepted for online bookings.",
                ["When do I pay the remaining amount?"] = "The remaining 50% balance plus any additional charges (damages, extra kilometers, late fees) are settled during the vehicle return process.",
                ["Can I get a refund?"] = "Refund policies depend on the timing and reason for cancellation. Contact our support team for specific refund requests and policy details."
            };
        }
    }
}