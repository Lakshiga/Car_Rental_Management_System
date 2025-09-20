namespace CarRentalManagementSystem.DTOs
{
    public class StaffResponseDTO
    {
        public int StaffID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsProfileComplete { get; set; }
        public bool RequirePasswordReset { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}