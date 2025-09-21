using CarRentalManagementSystem.DTOs;
using CarRentalManagementSystem.Services.Interfaces;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace CarRentalManagementSystem.Services
{
    public class AIAssistantService : IAIAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IBookingService _bookingService;
        private readonly ICarService _carService;
        private readonly IUserService _userService;
        private readonly CarRentalFAQService _faqService;
        private readonly ILogger<AIAssistantService> _logger;

        public AIAssistantService(
            HttpClient httpClient,
            IConfiguration configuration,
            IBookingService bookingService,
            ICarService carService,
            IUserService userService,
            CarRentalFAQService faqService,
            ILogger<AIAssistantService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _bookingService = bookingService;
            _carService = carService;
            _userService = userService;
            _faqService = faqService;
            _logger = logger;
        }

        public async Task<AIAssistantResponseDTO> ProcessMessageAsync(string message, string userType, string? userId = null)
        {
            try
            {
                // Validate and normalize user type
                userType = ValidateAndNormalizeUserType(userType);
                
                // Pre-filter message for role-appropriate content
                if (!IsMessageAppropriateForRole(message, userType))
                {
                    return new AIAssistantResponseDTO
                    {
                        Success = true,
                        Response = GetRoleRestrictedResponse(userType),
                        Timestamp = DateTime.UtcNow
                    };
                }

                // Get system prompt based on user type with strict role enforcement
                var systemPrompt = GetRoleSpecificSystemPrompt(userType);

                // Get contextual data for the user
                var contextData = await GetUserContextDataAsync(userType, userId);

                // Generate response using Gemini AI
                var response = await GenerateContextualResponseAsync(message, systemPrompt, contextData);
                
                // Post-process response to ensure role compliance
                response = FilterResponseForRole(response, userType);

                return new AIAssistantResponseDTO
                {
                    Success = true,
                    Response = response,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI assistant message");
                return new AIAssistantResponseDTO
                {
                    Success = false,
                    Response = "I apologize, but I'm currently experiencing technical difficulties. Please try again later.",
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<string> GenerateContextualResponseAsync(string message, string systemPrompt, string contextData)
        {
            try
            {
                var apiKey = _configuration["AIAssistantSettings:GeminiApiKey"];
                var modelName = _configuration["AIAssistantSettings:ModelName"] ?? "gemini-1.5-flash";
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("Gemini API key is not configured");
                    return "I'm currently experiencing configuration issues. Please contact support.";
                }
                
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

                var fullPrompt = $"{systemPrompt}\n\nContext Data:\n{contextData}\n\nUser Message: {message}\n\nPlease provide a helpful response based on the context and stay within the car rental domain.";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = fullPrompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = double.Parse(_configuration["AIAssistantSettings:Temperature"] ?? "0.7"),
                        maxOutputTokens = int.Parse(_configuration["AIAssistantSettings:MaxTokens"] ?? "2048")
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"Sending request to Gemini API: {url}");
                _logger.LogInformation($"Request body: {json}");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Gemini API response status: {response.StatusCode}");
                _logger.LogInformation($"Gemini API response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Raw JSON response: {responseContent}");
                    
                    try
                    {
                        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions 
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        _logger.LogInformation($"Parsed candidates count: {geminiResponse?.Candidates?.Count ?? 0}");
                        
                        var firstCandidate = geminiResponse?.Candidates?.FirstOrDefault();
                        _logger.LogInformation($"First candidate content: {firstCandidate?.Content != null}");
                        
                        var parts = firstCandidate?.Content?.Parts;
                        _logger.LogInformation($"Parts count: {parts?.Count ?? 0}");
                        
                        var aiResponse = parts?.FirstOrDefault()?.Text;
                        _logger.LogInformation($"Extracted AI response: '{aiResponse}'");
                        
                        if (!string.IsNullOrWhiteSpace(aiResponse))
                        {
                            return aiResponse.Trim();
                        }
                        
                        _logger.LogWarning("Gemini API returned empty or whitespace response");
                        return "I received your message but couldn't generate a proper response. Could you please rephrase your question?";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing Gemini response JSON");
                        return "I'm experiencing technical difficulties parsing the response. Please try again.";
                    }
                }
                else
                {
                    _logger.LogError($"Gemini API error: {response.StatusCode} - {responseContent}");
                    return "I'm currently experiencing some technical difficulties. Please try again in a moment.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return "I apologize for the inconvenience. There seems to be a technical issue. Please try again later.";
            }
        }

        public async Task<string> GetUserContextDataAsync(string userType, string? userId = null)
        {
            try
            {
                var contextBuilder = new StringBuilder();
                contextBuilder.AppendLine($"User Type: {userType}");
                contextBuilder.AppendLine($"Current Date: {DateTime.Now:yyyy-MM-dd}");

                if (userType.ToLower() == "customer" && !string.IsNullOrEmpty(userId))
                {
                    await BuildCustomerContext(contextBuilder, userId);
                }
                else if (userType.ToLower() == "admin")
                {
                    await BuildAdminContext(contextBuilder);
                }
                else if (userType.ToLower() == "staff")
                {
                    await BuildStaffContext(contextBuilder);
                }
                else if (userType.ToLower() == "guest")
                {
                    await BuildGuestContext(contextBuilder);
                }

                // Add general car rental information
                await BuildGeneralContext(contextBuilder);
                
                // Add enhanced FAQ knowledge
                var enhancedKnowledge = await _faqService.GetEnhancedKnowledgeBaseAsync();
                contextBuilder.AppendLine("\n=== ENHANCED FAQ KNOWLEDGE ===\n");
                contextBuilder.AppendLine(enhancedKnowledge);

                return contextBuilder.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building user context data");
                return $"User Type: {userType}\nCurrent Date: {DateTime.Now:yyyy-MM-dd}\nContext data temporarily unavailable.";
            }
        }

        private async Task BuildCustomerContext(StringBuilder contextBuilder, string userId)
        {
            try
            {
                // Get customer bookings
                if (int.TryParse(userId, out int customerId))
                {
                    var bookings = await _bookingService.GetBookingsByCustomerAsync(customerId);
                    contextBuilder.AppendLine($"Customer has {bookings.Count()} total bookings");
                    
                    var activeBookings = bookings.Where(b => b.Status == "Confirmed" || b.Status == "Approved" || b.Status == "Rented").ToList();
                    contextBuilder.AppendLine($"Active bookings: {activeBookings.Count}");
                    
                    var recentBookings = bookings.OrderByDescending(b => b.CreatedAt).Take(3);
                    contextBuilder.AppendLine("Recent bookings:");
                    foreach (var booking in recentBookings)
                    {
                        contextBuilder.AppendLine($"- {booking.CarDetails.CarName} from {booking.PickupDate:yyyy-MM-dd} to {booking.ReturnDate:yyyy-MM-dd} (Status: {booking.Status})");
                    }
                }

                contextBuilder.AppendLine("\nCustomer Dashboard Features Available:");
                contextBuilder.AppendLine("- View and manage bookings");
                contextBuilder.AppendLine("- Browse available cars");
                contextBuilder.AppendLine("- Make new reservations");
                contextBuilder.AppendLine("- Process payments");
                contextBuilder.AppendLine("- View booking history");
                contextBuilder.AppendLine("- Update profile information");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building customer context");
                contextBuilder.AppendLine("Customer context temporarily unavailable.");
            }
        }

        private async Task BuildStaffContext(StringBuilder contextBuilder)
        {
            try
            {
                // Get operational statistics (limited to staff scope)
                var allBookings = await _bookingService.GetAllBookingsAsync();
                var pendingBookings = allBookings.Where(b => b.Status == "Pending").Count();
                var approvedBookings = allBookings.Where(b => b.Status == "Approved").Count();
                var rentedBookings = allBookings.Where(b => b.Status == "Rented").Count();
                
                contextBuilder.AppendLine($"Pending booking requests: {pendingBookings}");
                contextBuilder.AppendLine($"Approved bookings ready for rental: {approvedBookings}");
                contextBuilder.AppendLine($"Currently rented vehicles: {rentedBookings}");

                var cars = await _carService.GetAllCarsAsync();
                var availableCars = cars.Where(c => c.AvailabilityStatus == "Available").Count();
                var rentedCars = cars.Where(c => c.AvailabilityStatus == "Rented").Count();
                contextBuilder.AppendLine($"Available cars for rental: {availableCars}");
                contextBuilder.AppendLine($"Cars currently rented: {rentedCars}");

                contextBuilder.AppendLine("\nStaff Operational Features Available:");
                contextBuilder.AppendLine("- Process booking requests (approve/reject)");
                contextBuilder.AppendLine("- Confirm vehicle rentals");
                contextBuilder.AppendLine("- Handle vehicle returns");
                contextBuilder.AppendLine("- Customer service support");
                contextBuilder.AppendLine("- View booking and rental history");
                contextBuilder.AppendLine("- Process return settlements");
                contextBuilder.AppendLine("- Generate rental receipts");
                
                contextBuilder.AppendLine("\nLimited Access: Staff cannot access admin-only functions like:");
                contextBuilder.AppendLine("- Staff management");
                contextBuilder.AppendLine("- System configuration");
                contextBuilder.AppendLine("- Business analytics/reports");
                contextBuilder.AppendLine("- Car inventory management (add/remove vehicles)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building staff context");
                contextBuilder.AppendLine("Staff operational context temporarily unavailable.");
            }
        }

        private async Task BuildAdminContext(StringBuilder contextBuilder)
        {
            try
            {
                // Get general statistics
                var allBookings = await _bookingService.GetAllBookingsAsync();
                var pendingBookings = allBookings.Where(b => b.Status == "Pending").Count();
                var approvedBookings = allBookings.Where(b => b.Status == "Approved").Count();
                
                contextBuilder.AppendLine($"Total bookings in system: {allBookings.Count()}");
                contextBuilder.AppendLine($"Pending approvals: {pendingBookings}");
                contextBuilder.AppendLine($"Approved bookings: {approvedBookings}");

                var cars = await _carService.GetAllCarsAsync();
                var availableCars = cars.Where(c => c.AvailabilityStatus == "Available").Count();
                contextBuilder.AppendLine($"Total cars: {cars.Count()}");
                contextBuilder.AppendLine($"Available cars: {availableCars}");

                contextBuilder.AppendLine("\nAdmin Dashboard Features Available:");
                contextBuilder.AppendLine("- Complete booking management (approve/reject/monitor)");
                contextBuilder.AppendLine("- Full car inventory management (add/edit/remove)");
                contextBuilder.AppendLine("- Comprehensive customer management");
                contextBuilder.AppendLine("- Payment processing oversight");
                contextBuilder.AppendLine("- Staff management (add/edit/remove staff)");
                contextBuilder.AppendLine("- Rental confirmations and processing");
                contextBuilder.AppendLine("- Return processing and settlements");
                contextBuilder.AppendLine("- System reports and analytics");
                contextBuilder.AppendLine("- Business intelligence and insights");
                contextBuilder.AppendLine("- System configuration and settings");
                contextBuilder.AppendLine("- Financial reports and management");
                contextBuilder.AppendLine("- Strategic decision support");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building admin context");
                contextBuilder.AppendLine("Admin context temporarily unavailable.");
            }
        }

        private async Task BuildGeneralContext(StringBuilder contextBuilder)
        {
            try
            {
                contextBuilder.AppendLine("\nGeneral Car Rental Information:");
                contextBuilder.AppendLine("- This is a car rental management system");
                contextBuilder.AppendLine("- Customers can book cars online");
                contextBuilder.AppendLine("- Payment processing available");
                contextBuilder.AppendLine("- Advance payment (50%) required for bookings");
                contextBuilder.AppendLine("- Admin approval required for bookings");
                contextBuilder.AppendLine("- Return settlement includes damage assessment");
                contextBuilder.AppendLine("- System tracks odometer readings");
                
                // Add available car information
                var cars = await _carService.GetAllCarsAsync();
                var availableCarBrands = cars.Where(c => c.AvailabilityStatus == "Available")
                                           .Select(c => c.Brand)
                                           .Distinct()
                                           .Take(5);
                contextBuilder.AppendLine($"Available car brands: {string.Join(", ", availableCarBrands)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building general context");
                contextBuilder.AppendLine("General context temporarily unavailable.");
            }
        }

        private async Task BuildGuestContext(StringBuilder contextBuilder)
        {
            try
            {
                contextBuilder.AppendLine("\nGeneral Car Rental Services:");
                contextBuilder.AppendLine("- Wide selection of vehicles for all occasions");
                contextBuilder.AppendLine("- Convenient online platform");
                contextBuilder.AppendLine("- Competitive pricing");
                contextBuilder.AppendLine("- Professional customer service");
                contextBuilder.AppendLine("- Flexible rental options");
                contextBuilder.AppendLine("- Various vehicle types available");
                
                // Get basic car information without revealing operational details
                var cars = await _carService.GetAllCarsAsync();
                var availableCars = cars.Where(c => c.AvailabilityStatus == "Available").Count();
                var carBrands = cars.Select(c => c.Brand).Distinct().Take(5);
                
                contextBuilder.AppendLine($"\nWe currently have vehicles available for rental");
                contextBuilder.AppendLine($"Popular vehicle brands include: {string.Join(", ", carBrands)}");
                
                contextBuilder.AppendLine("\nOur Services Include:");
                contextBuilder.AppendLine("- Short-term and long-term rentals");
                contextBuilder.AppendLine("- Various vehicle categories");
                contextBuilder.AppendLine("- Comprehensive vehicle information");
                contextBuilder.AppendLine("- Customer support assistance");
                contextBuilder.AppendLine("- Convenient pickup and return");
                
                contextBuilder.AppendLine("\nTo learn more about our rental services, pricing, or vehicle availability, feel free to ask!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building guest context");
                contextBuilder.AppendLine("Welcome to our car rental service! We offer a wide range of vehicles for rent.");
            }
        }

        private string ValidateAndNormalizeUserType(string userType)
        {
            return userType?.ToLower() switch
            {
                "customer" => "customer",
                "admin" => "admin",
                "staff" => "staff",
                "guest" or null or "" => "guest",
                _ => "guest" // Default to most restrictive
            };
        }

        private string GetRoleSpecificSystemPrompt(string userType)
        {
            return userType switch
            {
                "customer" => _configuration["AIAssistantSettings:SystemPromptCustomer"] ?? GetDefaultCustomerPrompt(),
                "admin" => _configuration["AIAssistantSettings:SystemPromptAdmin"] ?? GetDefaultAdminPrompt(),
                "staff" => _configuration["AIAssistantSettings:SystemPromptStaff"] ?? GetDefaultStaffPrompt(),
                "guest" => GetDefaultGuestPrompt(),
                _ => GetDefaultGuestPrompt()
            };
        }

        private string GetDefaultCustomerPrompt()
        {
            return "You are an AI assistant for customers of a car rental system. STRICT ROLE RESTRICTIONS: You can ONLY help with customer-specific functions: viewing/managing their own bookings, browsing available cars, making new reservations, processing their payments, viewing their rental history, and updating their profile. NEVER mention, discuss, or provide information about: admin functions, staff operations, business management, other customers' data, system administration, car inventory management, staff management, booking approvals, or any administrative processes. If asked about admin/staff topics, politely state you can only help with customer services and redirect to customer-related topics.";
        }

        private string GetDefaultAdminPrompt()
        {
            return "You are an AI assistant for administrators of a car rental system. STRICT ROLE RESTRICTIONS: You have FULL ACCESS to all administrative functions: managing bookings (approve/reject), complete car inventory management, comprehensive customer management, payment processing oversight, staff management, rental confirmations, return processing, system analytics, and all admin-only operations. You can discuss and provide information about: all booking operations, complete fleet management, customer data management, staff operations, business analytics, system configuration, and financial reports. ADMIN PRIVILEGES: You can view and manage all aspects of the system. Focus on helping with strategic decisions, system oversight, and comprehensive management tasks.";
        }

        private string GetDefaultStaffPrompt()
        {
            return "You are an AI assistant for staff members of a car rental system. STRICT ROLE RESTRICTIONS: You can help with OPERATIONAL tasks only: processing customer bookings (approve/reject customer requests), handling rental confirmations, managing returns, customer service tasks, and day-to-day operations. You can access and discuss: customer bookings and rental history, available car inventory, rental processes, and customer support. STAFF LIMITATIONS: You CANNOT access admin-only functions such as: staff management, system configuration, business analytics/reports, financial management, adding/removing cars from inventory, or any strategic admin decisions. NEVER mention, discuss, or provide information about admin-exclusive features. If asked about admin topics, politely state these require administrator access and redirect to operational tasks you can help with.";
        }

        private string GetDefaultGuestPrompt()
        {
            return "You are an AI assistant for a car rental company website helping visitors learn about services. STRICT ROLE RESTRICTIONS: You can ONLY provide general information about car rental services, available car types, and general company information. NEVER mention, discuss, or provide information about: customer accounts, admin functions, staff operations, booking processes, payment processes, account creation, login procedures, approval workflows, operational procedures, or any internal business processes. Focus ONLY on describing the car rental services in general terms - what types of vehicles are available, general service offerings, and basic company information. If asked about how to book, accounts, or operational processes, politely redirect to contacting customer service for specific assistance.";
        }

        private bool IsMessageAppropriateForRole(string message, string userType)
        {
            var lowerMessage = message.ToLower();
            
            // Define forbidden keywords for each role
            var adminOnlyKeywords = new[] { "staff management", "add staff", "remove staff", "delete staff", "system configuration", "business analytics", "financial reports", "admin settings", "system settings" };
            var adminAndStaffKeywords = new[] { "approve", "reject", "manage inventory", "manage cars", "manage customers", "dashboard admin", "booking approval", "approval process" };
            var customerKeywords = new[] { "customer dashboard", "my booking", "my account", "my payment", "personal", "login", "sign in", "create account", "register", "how to book", "booking process", "make reservation" };
            var staffKeywords = new[] { "staff", "employee" }; // Separate staff keywords for non-admin roles
            
            switch (userType)
            {
                case "customer":
                    // Customers shouldn't ask about admin or staff functions
                    if (adminOnlyKeywords.Any(keyword => lowerMessage.Contains(keyword)) ||
                        adminAndStaffKeywords.Any(keyword => lowerMessage.Contains(keyword)) ||
                        staffKeywords.Any(keyword => lowerMessage.Contains(keyword)) ||
                        lowerMessage.Contains("admin"))
                    {
                        return false;
                    }
                    break;
                    
                case "staff":
                    // Staff shouldn't ask about admin-only functions but can use staff-related terms
                    if (adminOnlyKeywords.Any(keyword => lowerMessage.Contains(keyword)) ||
                        lowerMessage.Contains("admin"))
                    {
                        return false;
                    }
                    break;
                    
                case "guest":
                    // Guests shouldn't ask about specific account, admin, or operational functions
                    if (adminOnlyKeywords.Any(keyword => lowerMessage.Contains(keyword)) ||
                        adminAndStaffKeywords.Any(keyword => lowerMessage.Contains(keyword)) ||
                        customerKeywords.Any(keyword => lowerMessage.Contains(keyword)) ||
                        staffKeywords.Any(keyword => lowerMessage.Contains(keyword)) ||
                        lowerMessage.Contains("admin"))
                    {
                        return false;
                    }
                    break;
                    
                case "admin":
                    // Admins can ask about anything within their scope - no restrictions
                    break;
            }
            
            return true;
        }

        private string GetRoleRestrictedResponse(string userType)
        {
            return userType switch
            {
                "customer" => "I can only help you with customer-related services like managing your bookings, browsing available cars, making reservations, and handling your payments. For administrative matters, please contact our staff directly.",
                "staff" => "I can help you with operational tasks like processing bookings, handling rentals and returns, and customer service. For administrative functions like staff management or system configuration, please contact an administrator.",
                "admin" => "I can help you with all administrative functions including managing bookings, car inventory, customer management, staff management, and system configuration. How can I assist you today?",
                "guest" => "I can only provide general information about our car rental services. To access specific account features or get personalized assistance, please create an account or sign in.",
                _ => "I can only provide general information about our car rental services. Please specify how I can help you today."
            };
        }

        private string FilterResponseForRole(string response, string userType)
        {
            // Additional post-processing to ensure no role-inappropriate information leaks
            var filteredResponse = response;
            
            switch (userType)
            {
                case "customer":
                    // Remove any admin-specific terminology that might have slipped through
                    filteredResponse = RemoveAdminTerminology(filteredResponse);
                    filteredResponse = RemoveStaffTerminology(filteredResponse);
                    break;
                    
                case "staff":
                    // Remove admin-only terminology but keep staff-appropriate terms
                    filteredResponse = RemoveAdminOnlyTerminology(filteredResponse);
                    break;
                    
                case "guest":
                    // Remove any account-specific or admin terminology
                    filteredResponse = RemoveAccountSpecificTerminology(filteredResponse);
                    filteredResponse = RemoveAdminTerminology(filteredResponse);
                    filteredResponse = RemoveStaffTerminology(filteredResponse);
                    break;
                    
                case "admin":
                    // Admin responses can contain all administrative information
                    break;
            }
            
            return filteredResponse;
        }

        private string RemoveAdminOnlyTerminology(string response)
        {
            var adminOnlyTerms = new Dictionary<string, string>
            {
                { "staff management", "operational support" },
                { "add staff", "team coordination" },
                { "remove staff", "team coordination" },
                { "delete staff", "team coordination" },
                { "system configuration", "system operations" },
                { "business analytics", "operational metrics" },
                { "financial reports", "payment processing" },
                { "admin settings", "system features" }
            };
            
            foreach (var term in adminOnlyTerms)
            {
                response = response.Replace(term.Key, term.Value, StringComparison.OrdinalIgnoreCase);
            }
            
            return response;
        }

        private string RemoveStaffTerminology(string response)
        {
            var staffTerms = new Dictionary<string, string>
            {
                { "staff dashboard", "our services" },
                { "staff operations", "service operations" },
                { "staff access", "service access" },
                { "operational tasks", "service tasks" },
                { "staff", "our team" },
                { "employee", "team member" },
                { "booking approval", "rental confirmation" },
                { "approval workflow", "rental process" }
            };
            
            foreach (var term in staffTerms)
            {
                response = response.Replace(term.Key, term.Value, StringComparison.OrdinalIgnoreCase);
            }
            
            return response;
        }

        private string RemoveAdminTerminology(string response)
        {
            var adminTerms = new Dictionary<string, string>
            {
                { "admin dashboard", "our services" },
                { "approve bookings", "process requests" },
                { "reject bookings", "handle requests" },
                { "staff management", "team operations" },
                { "inventory management", "vehicle management" },
                { "admin approval", "processing" },
                { "administrator", "staff" },
                { "admin", "staff" },
                { "approval process", "rental process" },
                { "booking approval", "rental confirmation" }
            };
            
            foreach (var term in adminTerms)
            {
                response = response.Replace(term.Key, term.Value, StringComparison.OrdinalIgnoreCase);
            }
            
            return response;
        }

        private string RemoveAccountSpecificTerminology(string response)
        {
            var accountTerms = new Dictionary<string, string>
            {
                { "your booking", "car bookings" },
                { "your account", "customer accounts" },
                { "your payment", "payment processing" },
                { "my booking", "car bookings" },
                { "my account", "customer accounts" },
                { "create an account", "contact us" },
                { "sign in", "get assistance" },
                { "login", "contact us" },
                { "register", "inquire" },
                { "booking process", "rental services" },
                { "submit booking", "inquire about rentals" },
                { "approval process", "rental process" },
                { "admin approval", "processing" },
                { "wait for approval", "rental confirmation" },
                { "booking request", "rental inquiry" }
            };
            
            foreach (var term in accountTerms)
            {
                response = response.Replace(term.Key, term.Value, StringComparison.OrdinalIgnoreCase);
            }
            
            return response;
        }

        // Helper classes for Gemini API response
        private class GeminiResponse
        {
            public List<Candidate>? Candidates { get; set; }
        }

        private class Candidate
        {
            public Content? Content { get; set; }
            public string? FinishReason { get; set; }
        }

        private class Content
        {
            public List<Part>? Parts { get; set; }
            public string? Role { get; set; }
        }

        private class Part
        {
            public string? Text { get; set; }
        }
    }
}