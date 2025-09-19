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
                string password = emailSettings["Password"]!;

                using var message = new MailMessage(fromEmail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                using var smtp = new SmtpClient(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]!))
                {
                    Credentials = new NetworkCredential(fromEmail, password),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email Error: " + ex.Message);
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

        public async Task<bool> SendBookingRejectionAsync(string customerEmail, string customerName, int bookingId)
        {
            var subject = "Booking Update - Car Rental System";
            var body = $@"
                <h2>Booking Status Update</h2>
                <p>Dear {customerName},</p>
                <p>We regret to inform you that your booking request could not be approved at this time.</p>
                <p><strong>Booking ID:</strong> {bookingId}</p>
                <p>This could be due to car unavailability or other factors. Please feel free to make a new booking or contact us for assistance.</p>
                <p>Thank you for your understanding.</p>
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
    }
}