using CarRentalManagementSystem.DTOs;

namespace CarRentalManagementSystem.Services.Interfaces
{
    public interface IAIAssistantService
    {
        Task<AIAssistantResponseDTO> ProcessMessageAsync(string message, string userType, string? userId = null);
        Task<string> GenerateContextualResponseAsync(string message, string systemPrompt, string contextData);
        Task<string> GetUserContextDataAsync(string userType, string? userId = null);
    }
}