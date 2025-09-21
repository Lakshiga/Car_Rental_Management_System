namespace CarRentalManagementSystem.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendBookingConfirmationAsync(string customerEmail, string customerName, int bookingId);
        Task<bool> SendBookingApprovalAsync(string customerEmail, string customerName, int bookingId);
        Task<bool> SendBookingRejectionAsync(string customerEmail, string customerName, int bookingId, string rejectionReason, decimal advanceAmount);
        Task<bool> SendContactAcknowledgmentAsync(string customerEmail, string customerName);
        Task<bool> SendStaffCredentialsAsync(string staffEmail, string staffName, string username, string password);
    }
}