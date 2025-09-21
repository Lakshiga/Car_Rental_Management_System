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
                else if (userType.ToLower() == "admin" || userType.ToLower() == "staff")
                {
                    await BuildAdminContext(contextBuilder);
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
                contextBuilder.AppendLine("- Manage bookings (approve/reject)");
                contextBuilder.AppendLine("- Car inventory management");
                contextBuilder.AppendLine("- Customer management");
                contextBuilder.AppendLine("- Payment processing");
                contextBuilder.AppendLine("- Staff management");
                contextBuilder.AppendLine("- Rental confirmations");
                contextBuilder.AppendLine("- Return processing");
                contextBuilder.AppendLine("- System reports and analytics");
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
                contextBuilder.AppendLine("- Wide selection of cars available for rental");
                contextBuilder.AppendLine("- Easy online booking process");
                contextBuilder.AppendLine("- Competitive rental rates");
                contextBuilder.AppendLine("- Professional customer service");
                contextBuilder.AppendLine("- Flexible rental periods");
                
                // Get basic car information
                var cars = await _carService.GetAllCarsAsync();
                var availableCars = cars.Where(c => c.AvailabilityStatus == "Available").Count();
                var carBrands = cars.Select(c => c.Brand).Distinct().Take(5);
                
                contextBuilder.AppendLine($"\nCurrently {availableCars} cars available for rental");
                contextBuilder.AppendLine($"Popular brands: {string.Join(", ", carBrands)}");
                
                contextBuilder.AppendLine("\nHow to Get Started:");
                contextBuilder.AppendLine("1. Browse available cars");
                contextBuilder.AppendLine("2. Create an account or sign in");
                contextBuilder.AppendLine("3. Select your rental dates");
                contextBuilder.AppendLine("4. Submit booking request");
                contextBuilder.AppendLine("5. Wait for admin approval");
                contextBuilder.AppendLine("6. Make advance payment (50%)");
                contextBuilder.AppendLine("7. Pick up your car on scheduled date");
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
                "admin" or "staff" => "admin",
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
            return "You are an AI assistant for administrators and staff of a car rental system. STRICT ROLE RESTRICTIONS: You can ONLY help with admin/staff functions: managing bookings (approve/reject), car inventory management, customer management, payment processing oversight, staff management, rental confirmations, return processing, and system analytics. NEVER mention, discuss, or provide information about: specific customer personal details (unless directly relevant to admin tasks), customer-only features, or anything outside administrative scope. If asked about customer-specific topics that admins shouldn't handle, redirect to appropriate admin functions.";
        }

        private string GetDefaultGuestPrompt()
        {
            return "You are an AI assistant for a car rental company website helping visitors learn about services. STRICT ROLE RESTRICTIONS: You can ONLY provide general information about car rental services, available car types, booking process overview, and general company information. NEVER mention, discuss, or provide information about: specific customer accounts, admin functions, staff operations, detailed pricing, booking management, customer data, or administrative processes. Focus only on helping visitors understand your services and how to get started as a potential customer. If asked about specific account or admin topics, politely redirect to contacting customer service or creating an account.";
        }

        private bool IsMessageAppropriateForRole(string message, string userType)
        {
            var lowerMessage = message.ToLower();
            
            // Define forbidden keywords for each role
            var adminKeywords = new[] { "admin", "staff", "approve", "reject", "manage inventory", "manage cars", "manage customers", "staff management", "dashboard admin", "system admin" };
            var customerKeywords = new[] { "customer dashboard", "my booking", "my account", "my payment", "personal" };
            
            switch (userType)
            {
                case "customer":
                    // Customers shouldn't ask about admin functions
                    if (adminKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                    {
                        return false;
                    }
                    break;
                    
                case "guest":
                    // Guests shouldn't ask about specific account or admin functions
                    if (adminKeywords.Any(keyword => lowerMessage.Contains(keyword)) ||
                        customerKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                    {
                        return false;
                    }
                    break;
                    
                case "admin":
                    // Admins can ask about anything within their scope
                    break;
            }
            
            return true;
        }

        private string GetRoleRestrictedResponse(string userType)
        {
            return userType switch
            {
                "customer" => "I can only help you with customer-related services like managing your bookings, browsing available cars, making reservations, and handling your payments. For administrative matters, please contact our staff directly.",
                "guest" => "I can only provide general information about our car rental services. To access specific account features or get personalized assistance, please create an account or sign in.",
                "admin" => "I can help you with administrative functions like managing bookings, car inventory, and customer management. How can I assist you today?",
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
                    break;
                    
                case "guest":
                    // Remove any account-specific or admin terminology
                    filteredResponse = RemoveAccountSpecificTerminology(filteredResponse);
                    filteredResponse = RemoveAdminTerminology(filteredResponse);
                    break;
                    
                case "admin":
                    // Admin responses can contain administrative information
                    break;
            }
            
            return filteredResponse;
        }

        private string RemoveAdminTerminology(string response)
        {
            var adminTerms = new Dictionary<string, string>
            {
                { "admin dashboard", "management system" },
                { "approve bookings", "process bookings" },
                { "reject bookings", "handle bookings" },
                { "staff management", "team operations" },
                { "inventory management", "car management" }
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
                { "my account", "customer accounts" }
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