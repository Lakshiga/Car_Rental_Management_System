namespace CarRentalManagementSystem.DTOs
{
    public class AIAssistantRequestDTO
    {
        public string Message { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty; // "customer" or "admin"
        public string? UserId { get; set; }
    }

    public class AIAssistantResponseDTO
    {
        public bool Success { get; set; }
        public string Response { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class AIAssistantContextDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public List<string> RecentBookings { get; set; } = new();
        public List<string> AvailableCars { get; set; } = new();
        public List<string> RecentActivities { get; set; } = new();
        public Dictionary<string, object> AdditionalContext { get; set; } = new();
    }
}