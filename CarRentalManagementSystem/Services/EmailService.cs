using System.Net;
using System.Net.Mail;
using CarRentalManagementSystem.Services.Interfaces;

namespace CarRentalManagementSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                string fromEmail = emailSettings["From"]!;
                string username = emailSettings["Username"] ?? fromEmail;
                string password = emailSettings["Password"]!;

                using var message = new MailMessage(fromEmail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                using var smtp = new SmtpClient(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]!))
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return false;
            }
        }

        public async Task<bool> SendBookingConfirmationAsync(string customerEmail, string customerName, int bookingId)
        {
            var subject = "Booking Confirmation - Car Rental System";
            var body = $@"
                <h2>Booking Confirmation</h2>
                <p>Dear {customerName},</p>
                <p>Your booking request has been received successfully!</p>
                <p><strong>Booking ID:</strong> {bookingId}</p>
                <p>Your booking is currently pending approval. We will notify you once it's approved.</p>
                <p>Thank you for choosing our car rental service!</p>
                <br>
                <p>Best regards,<br>Car Rental Management Team</p>
            ";

            return await SendEmailAsync(customerEmail, subject, body);
        }

        public async Task<bool> SendBookingApprovalAsync(string customerEmail, string customerName, int bookingId)
        {
            var subject = "Booking Approved - Car Rental System";
            var body = $@"
                <h2>Booking Approved!</h2>
                <p>Dear {customerName},</p>
                <p>Great news! Your booking has been approved.</p>
                <p><strong>Booking ID:</strong> {bookingId}</p>
                <p>You can now proceed with the rental process. Please visit our office at the scheduled pickup time.</p>
                <p>Thank you for choosing our car rental service!</p>
                <br>
                <p>Best regards,<br>Car Rental Management Team</p>
            ";

            return await SendEmailAsync(customerEmail, subject, body);
        }

        public async Task<bool> SendBookingRejectionAsync(string customerEmail, string customerName, int bookingId, string rejectionReason, decimal advanceAmount)
        {
            var subject = "Booking Rejected - Refund Information - Car Rental System";
            var body = $@"
                <h2>Booking Rejection Notice</h2>
                <p>Dear {customerName},</p>
                <p>We regret to inform you that your booking request has been rejected.</p>
                
                <div style=""background-color: #f8f9fa; padding: 20px; margin: 20px 0; border-left: 4px solid #dc3545;"">
                    <h3>Booking Details:</h3>
                    <p><strong>Booking ID:</strong> #{bookingId}</p>
                    <p><strong>Rejection Reason:</strong> {rejectionReason}</p>
                    <p><strong>Advance Payment Amount:</strong> ₹{advanceAmount:F2}</p>
                </div>
                
                <div style=""background-color: #d4edda; padding: 20px; margin: 20px 0; border-left: 4px solid #28a745;"">
                    <h3>💰 Refund Information:</h3>
                    <p><strong>Your advance payment of ₹{advanceAmount:F2} will be refunded.</strong></p>
                    <p><strong>Important:</strong> To collect your refund, please visit our office with:</p>
                    <ul>
                        <li>Valid government-issued photo ID</li>
                        <li>Original payment receipt or booking confirmation</li>
                        <li>Bank account details for transfer (if applicable)</li>
                    </ul>
                    <p><strong>Office Hours:</strong> Monday to Friday, 9:00 AM - 6:00 PM</p>
                    <p><strong>Office Address:</strong> [Your Office Address Here]</p>
                    <p><strong>Contact:</strong> +1 (555) 123-4567</p>
                </div>
                
                <div style=""background-color: #fff3cd; padding: 15px; margin: 20px 0; border-left: 4px solid #ffc107;"">
                    <h4>📝 Next Steps:</h4>
                    <ul>
                        <li>Visit our office within 30 days to collect your refund</li>
                        <li>Feel free to make a new booking with different dates or vehicle</li>
                        <li>Contact us if you have any questions about this rejection</li>
                    </ul>
                </div>
                
                <p>We apologize for any inconvenience caused and thank you for your understanding.</p>
                <p>If you have any questions, please don't hesitate to contact us.</p>
                <br>
                <p>Best regards,<br>Car Rental Management Team</p>
            ";

            return await SendEmailAsync(customerEmail, subject, body);
        }

        public async Task<bool> SendContactAcknowledgmentAsync(string customerEmail, string customerName)
        {
            var subject = "Thank you for contacting us - Car Rental System";
            var body = $@"
                <h2>Thank you for reaching out!</h2>
                <p>Dear {customerName},</p>
                <p>We have received your message and will get back to you within 24 hours.</p>
                <p>If you have any urgent queries, please call us at +1 (555) 123-4567.</p>
                <p>Thank you for choosing our car rental service!</p>
                <br>
                <p>Best regards,<br>Car Rental Management Team</p>
            ";

            return await SendEmailAsync(customerEmail, subject, body);
        }

        public async Task<bool> SendStaffCredentialsAsync(string staffEmail, string staffName, string username, string password)
        {
            Console.WriteLine($"Sending staff credentials email to: {staffEmail}");
            
            var subject = "Welcome to Car Rental Management System - Your Login Credentials";
            var body = $@"
                <h2>Welcome to Our Team!</h2>
                <p>Dear {staffName},</p>
                <p>Welcome to the Car Rental Management System! Your staff account has been created successfully.</p>
                
                <div style=""background-color: #f8f9fa; padding: 20px; margin: 20px 0; border-left: 4px solid #007bff;"">
                    <h3>Your Login Credentials:</h3>
                    <p><strong>Username:</strong> {username}</p>
                    <p><strong>Temporary Password:</strong> {password}</p>
                </div>
                
                <div style=""background-color: #fff3cd; padding: 15px; margin: 20px 0; border-left: 4px solid #ffc107;"">
                    <h4>⚠️ Important Security Notice:</h4>
                    <ul>
                        <li>You will be required to reset your password on first login</li>
                        <li>You must complete your profile before accessing any system features</li>
                        <li>Please keep your credentials secure and do not share them</li>
                    </ul>
                </div>
                
                <p><strong>Login URL:</strong> <a href=""#"">Car Rental Admin Portal</a></p>
                
                <p>If you have any questions or need assistance, please contact the administrator.</p>
                
                <p>Best regards,<br>Car Rental Management Team</p>
            ";

            var result = await SendEmailAsync(staffEmail, subject, body);
            Console.WriteLine($"Email send result: {result}");
            return result;
        }
    }
}