using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Services.Interfaces;

namespace CarRentalManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIAssistantController : ControllerBase
    {
        private readonly IAIAssistantService _aiAssistantService;
        private readonly ILogger<AIAssistantController> _logger;

        public AIAssistantController(
            IAIAssistantService aiAssistantService,
            ILogger<AIAssistantController> logger)
        {
            _aiAssistantService = aiAssistantService;
            _logger = logger;
        }

        [HttpPost("customer")]
        public async Task<IActionResult> ProcessCustomerMessage([FromBody] AIAssistantRequestDTO request)
        {
            try
            {
                // Strict role validation - only allow if user is actually a customer
                var customerId = HttpContext.Session.GetString("CustomerId");
                var userRole = HttpContext.Session.GetString("UserRole");
                
                // Ensure user is logged in as a customer
                if (string.IsNullOrEmpty(customerId) || userRole != "Customer")
                {
                    return Unauthorized(new AIAssistantResponseDTO
                    {
                        Success = false,
                        Response = "Access denied. This service is only available to logged-in customers.",
                        ErrorMessage = "Unauthorized access"
                    });
                }

                // Validate request
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new AIAssistantResponseDTO
                    {
                        Success = false,
                        Response = "Please provide a message.",
                        ErrorMessage = "Message is required"
                    });
                }

                // Process message with customer context
                var response = await _aiAssistantService.ProcessMessageAsync(
                    request.Message, 
                    "customer", 
                    customerId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing customer AI assistant message");
                return StatusCode(500, new AIAssistantResponseDTO
                {
                    Success = false,
                    Response = "I'm experiencing technical difficulties. Please try again later.",
                    ErrorMessage = "Internal server error"
                });
            }
        }

        [HttpPost("admin")]
        public async Task<IActionResult> ProcessAdminMessage([FromBody] AIAssistantRequestDTO request)
        {
            try
            {
                // Strict role validation - only allow if user is admin or staff
                var userRole = HttpContext.Session.GetString("UserRole");
                var staffId = HttpContext.Session.GetString("StaffId");
                var adminId = HttpContext.Session.GetString("AdminId");
                
                // Ensure user is logged in as admin or staff
                if (userRole != "Admin" && userRole != "Staff")
                {
                    return Forbid("Access denied. This service is only available to administrators and staff.");
                }
                
                // Ensure they have a valid ID
                if (string.IsNullOrEmpty(staffId) && string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(new AIAssistantResponseDTO
                    {
                        Success = false,
                        Response = "Access denied. Invalid administrative session.",
                        ErrorMessage = "Invalid session"
                    });
                }

                // Validate request
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new AIAssistantResponseDTO
                    {
                        Success = false,
                        Response = "Please provide a message.",
                        ErrorMessage = "Message is required"
                    });
                }

                // Get user ID from session (staff or admin)
                var userId = staffId ?? adminId;

                // Process message with admin context
                var response = await _aiAssistantService.ProcessMessageAsync(
                    request.Message, 
                    "admin", // Always use "admin" for both admin and staff
                    userId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing admin AI assistant message");
                return StatusCode(500, new AIAssistantResponseDTO
                {
                    Success = false,
                    Response = "I'm experiencing technical difficulties. Please try again later.",
                    ErrorMessage = "Internal server error"
                });
            }
        }

        [HttpPost("guest")]
        public async Task<IActionResult> ProcessGuestMessage([FromBody] AIAssistantRequestDTO request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new AIAssistantResponseDTO
                    {
                        Success = false,
                        Response = "Please provide a message.",
                        ErrorMessage = "Message is required"
                    });
                }

                // Optional: Verify user is not already logged in with a specific role
                var userRole = HttpContext.Session.GetString("UserRole");
                var customerId = HttpContext.Session.GetString("CustomerId");
                
                // If user is logged in, redirect them to appropriate endpoint
                if (!string.IsNullOrEmpty(userRole) || !string.IsNullOrEmpty(customerId))
                {
                    var redirectMessage = userRole switch
                    {
                        "Customer" => "As a logged-in customer, please use the customer chat service for personalized assistance.",
                        "Admin" or "Staff" => "As an administrator, please use the admin chat service for administrative functions.",
                        _ => "Please use the appropriate chat service based on your account type."
                    };
                    
                    return Ok(new AIAssistantResponseDTO
                    {
                        Success = true,
                        Response = redirectMessage,
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Process message with guest context (general car rental information)
                var response = await _aiAssistantService.ProcessMessageAsync(
                    request.Message, 
                    "guest", 
                    null);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing guest AI assistant message");
                return StatusCode(500, new AIAssistantResponseDTO
                {
                    Success = false,
                    Response = "I'm experiencing technical difficulties. Please try again later.",
                    ErrorMessage = "Internal server error"
                });
            }
        }

        [HttpGet("context/customer")]
        public async Task<IActionResult> GetCustomerContext()
        {
            try
            {
                var customerId = HttpContext.Session.GetString("CustomerId");
                var contextData = await _aiAssistantService.GetUserContextDataAsync("customer", customerId);
                
                return Ok(new { context = contextData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer context");
                return StatusCode(500, new { error = "Unable to retrieve context" });
            }
        }

        [HttpGet("context/admin")]
        public async Task<IActionResult> GetAdminContext()
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Admin" && userRole != "Staff")
                {
                    return Forbid();
                }

                var userId = HttpContext.Session.GetString("StaffId") ?? 
                           HttpContext.Session.GetString("AdminId");
                var contextData = await _aiAssistantService.GetUserContextDataAsync(userRole.ToLower(), userId);
                
                return Ok(new { context = contextData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin context");
                return StatusCode(500, new { error = "Unable to retrieve context" });
            }
        }

        [HttpPost("feedback")]
        public IActionResult SubmitFeedback([FromBody] dynamic feedbackData)
        {
            try
            {
                // Log feedback for improving the AI assistant
                _logger.LogInformation($"AI Assistant Feedback: {feedbackData}");
                
                return Ok(new { success = true, message = "Thank you for your feedback!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI assistant feedback");
                return StatusCode(500, new { success = false, message = "Unable to process feedback" });
            }
        }
    }
}