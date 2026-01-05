using GateSale.Models;
using GateSale.Services.Interfaces;
using System.Net.Http.Json;

namespace GateSale.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private static readonly List<User> _users = new();
        private static readonly List<UserProfile> _userProfiles = new();
        private static readonly List<UserPreferences> _userPreferences = new();
        private static User? _currentUser = null;
        private static string? _authToken = null;
        private static bool _isInitialized = false;

        public UserService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("GateSaleAPI");
        }

        private void InitializeData()
        {
            // Initialize sample users
            var user1 = new User
            {
                Id = "user1",
                FirstName = "Petra",
                LastName = "Johnson",
                Email = "petra.johnson@lincolnhigh.edu",
                PhoneNumber = "+1234567890",
                SchoolId = "lincoln-high",
                Grade = "Grade 11",
                ProfileImageUrl = "/images/profile-petra.jpg",
                IsVerified = true,
                IsActive = true,
                CreatedAt = DateTime.Now.AddMonths(-6),
                LastLoginAt = DateTime.Now.AddHours(-2)
            };

            var user2 = new User
            {
                Id = "user2",
                FirstName = "Alex",
                LastName = "Smith",
                Email = "alex.smith@lincolnhigh.edu",
                PhoneNumber = "+1234567891",
                SchoolId = "lincoln-high",
                Grade = "Grade 12",
                ProfileImageUrl = "/images/profile-alex.jpg",
                IsVerified = true,
                IsActive = true,
                CreatedAt = DateTime.Now.AddMonths(-4),
                LastLoginAt = DateTime.Now.AddDays(-1)
            };

            var user3 = new User
            {
                Id = "user3",
                FirstName = "Sarah",
                LastName = "Davis",
                Email = "sarah.davis@lincolnhigh.edu",
                PhoneNumber = "+1234567892",
                SchoolId = "lincoln-high",
                Grade = "Grade 10",
                ProfileImageUrl = "/images/profile-sarah.jpg",
                IsVerified = false,
                IsActive = true,
                CreatedAt = DateTime.Now.AddMonths(-2),
                LastLoginAt = DateTime.Now.AddHours(-5)
            };

            _users.AddRange(new[] { user1, user2, user3 });

            // Set current user (Petra from Profile.razor)
            _currentUser = user1;

            // Initialize user profiles
            _userProfiles.Add(new UserProfile
            {
                UserId = "user1",
                DisplayName = "Petra J.",
                Bio = "Grade 11 student at Lincoln High School. Love reading and math!",
                ProfileImageUrl = "/images/profile-petra.jpg",
                Grade = "Grade 11",
                SchoolId = "lincoln-high",
                Interests = new List<string> { "Mathematics", "Science", "Reading", "Technology" },
                Stats = new UserStats
                {
                    ItemsSold = 5,
                    ItemsBought = 12,
                    FavoriteItems = 8,
                    TotalEarnings = 245.50m,
                    TotalSpent = 389.99m,
                    AverageRating = 4.8,
                    ReviewCount = 15
                },
                Settings = new UserSettings
                {
                    NotificationsEnabled = true,
                    EmailNotifications = true,
                    PushNotifications = true,
                    ShowOnlineStatus = true,
                    PreferredLanguage = "en",
                    Currency = "USD"
                }
            });

            // Initialize user preferences
            _userPreferences.Add(new UserPreferences
            {
                UserId = "user1",
                PreferredCategories = new List<string> { "Textbooks", "Electronics", "School Supplies" },
                PreferredGrades = new List<string> { "Grade 11", "Grade 12" },
                MaxBudget = 200,
                MinBudget = 10,
                PreferredConditions = new List<string> { "New", "Like New", "Good" },
                OnlyVerifiedSellers = true,
                OnlySameCourse = false
            });
        }

        // User Management Methods
        public async Task<User?> GetCurrentUserAsync()
        {
            // TEMP: Skip cache to debug
            // if (_currentUser != null) return _currentUser;
            System.Diagnostics.Debug.WriteLine($"[GetCurrentUserAsync] Starting fresh fetch (cache disabled for debug)");

            try
            {
                var token = await GetAuthTokenAsync();
                System.Diagnostics.Debug.WriteLine($"[GetCurrentUserAsync] Token exists: {!string.IsNullOrEmpty(token)}");
                if (string.IsNullOrEmpty(token)) return null;

                var response = await _httpClient.GetAsync("api/User/profile");
                System.Diagnostics.Debug.WriteLine($"[GetCurrentUserAsync] API Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[GetCurrentUserAsync] API Response Content: {content}");
                    
                    var profile = System.Text.Json.JsonSerializer.Deserialize<UserProfileResponseDto>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    System.Diagnostics.Debug.WriteLine($"[GetCurrentUserAsync] Profile parsed: {profile != null}, FullName: '{profile?.FullName}', Grade: {profile?.Grade}, SchoolName: '{profile?.SchoolName}'");
                    
                    if (profile != null)
                    {
                        var firstName = !string.IsNullOrEmpty(profile.FullName) && profile.FullName.Contains(' ') ? profile.FullName.Split(' ')[0] : (profile.FullName ?? "");
                        var lastName = !string.IsNullOrEmpty(profile.FullName) && profile.FullName.Contains(' ') ? profile.FullName.Split(' ')[1] : "";

                        _currentUser = new User
                        {
                            Id = profile.Id.ToString(),
                            FirstName = firstName,
                            LastName = lastName,
                            Email = profile.Email ?? "",
                            PhoneNumber = profile.PhoneNumber ?? "",
                            Grade = profile.Grade.ToString(),
                            SchoolName = profile.SchoolName ?? "",
                            ProfileImageUrl = profile.ProfileImageUrl ?? "",
                            IsVerified = profile.IsEmailVerified,
                            IsMinor = profile.IsMinor,
                            ParentalConsentGiven = profile.ParentalConsentGiven
                        };
                        System.Diagnostics.Debug.WriteLine($"[GetCurrentUserAsync] User created: FirstName='{_currentUser.FirstName}', SchoolName='{_currentUser.SchoolName}', Grade='{_currentUser.Grade}'");
                        return _currentUser;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    System.Diagnostics.Debug.WriteLine("[GetCurrentUserAsync] Unauthorized - logging out");
                    await LogoutUserAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[GetCurrentUserAsync] API Error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetCurrentUserAsync] Exception: {ex.Message}");
            }

            return null;
        }

        private class UserProfileResponseDto
        {
            public Guid Id { get; set; }
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public string? PhoneNumber { get; set; }
            public int Grade { get; set; }
            public string? ProfileImageUrl { get; set; }
            public bool IsEmailVerified { get; set; }
            public bool IsMinor { get; set; }
            public bool ParentalConsentGiven { get; set; }
            public string? SchoolName { get; set; }
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            await Task.Delay(1);
            return _users.FirstOrDefault(u => u.Id == userId && u.IsActive);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            await Task.Delay(1);
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser == null) return false;

            var index = _users.IndexOf(existingUser);
            user.UpdatedAt = DateTime.Now;
            _users[index] = user;

            // Update current user if it's the same
            if (_currentUser?.Id == user.Id)
            {
                _currentUser = user;
            }

            return true;
        }

        public async Task<bool> SetCurrentUserAsync(string userId)
        {
            await Task.Delay(1);
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            _currentUser = user;
            user.LastLoginAt = DateTime.Now;
            return true;
        }

        // Authentication Methods
        public async Task<bool> IsUserLoggedInAsync()
        {
            await Task.Delay(1);
            return _currentUser != null;
        }

        public async Task<(bool Success, string Message)> LoginUserAsync(string email, string password)
        {
            try
            {
                var loginDto = new { Email = email, Password = password };
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginDto);

                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<GateSale.Models.AuthResponseApiDto>();
                    if (authResponse != null)
                    {
                        _authToken = authResponse.Token;
                        // Store in SecureStorage for persistence
                        await SecureStorage.Default.SetAsync("auth_token", _authToken);
                        
                        // Let's try to find the user in our local list for now to keep compatibility
                        var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                        if (user != null)
                        {
                            _currentUser = user;
                            user.LastLoginAt = DateTime.Now;
                        }
                        else
                        {
                            // If not in local list, create a temporary one from the response
                            _currentUser = new User 
                            { 
                                Id = authResponse.User.Id.ToString(),
                                Email = authResponse.User.Email, 
                                FirstName = authResponse.User.FullName.Split(' ')[0], 
                                LastName = authResponse.User.FullName.Contains(' ') ? authResponse.User.FullName.Split(' ')[1] : "" 
                            };
                        }

                        return (true, "Login successful.");
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(errorContent);
                    if (errorObj.TryGetProperty("message", out var msg))
                    {
                        return (false, msg.GetString() ?? "Login failed.");
                    }
                }
                catch { }

                return (false, "Invalid email or password.");
            }
            catch (Exception ex)
            {
                return (false, $"Connection error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> RegisterUserAsync(User user, string password)
        {
            try
            {
                var registerDto = new
                {
                    Email = user.Email,
                    Password = password,
                    FullName = $"{user.FirstName} {user.LastName}",
                    School = user.SchoolId, // Assuming SchoolId is the school name for now
                    Grade = int.TryParse(user.Grade, out var g) ? g : 8,
                    DateOfBirth = DateTime.SpecifyKind(user.DateOfBirth, DateTimeKind.Utc),
                    ParentEmail = user.PhoneNumber // Passed from Signup.razor
                };

                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", registerDto);

                if (response.IsSuccessStatusCode)
                {
                    // For now, we still add to local list for compatibility with other mock services
                    if (string.IsNullOrEmpty(user.Id))
                        user.Id = Guid.NewGuid().ToString();

                    user.CreatedAt = DateTime.Now;
                    user.UpdatedAt = DateTime.Now;
                    user.IsActive = true;

                    _users.Add(user);
                    _currentUser = user;
                    return (true, "Registration successful.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                try 
                {
                    var errorObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(errorContent);
                    if (errorObj.TryGetProperty("message", out var msg))
                    {
                        return (false, msg.GetString() ?? "Registration failed.");
                    }
                }
                catch { }

                return (false, $"Error: {response.StatusCode}. {errorContent}");
            }
            catch (Exception ex)
            {
                return (false, $"Connection error: {ex.Message}");
            }
        }

        public async Task<bool> VerifyEmailAsync(string email, string code)
        {
            try
            {
                var verifyDto = new { Email = email, Code = code };
                var response = await _httpClient.PostAsJsonAsync("api/Auth/verify-email", verifyDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> VerifyResetCodeAsync(string email, string code)
        {
            try
            {
                var verifyDto = new { Email = email, Code = code };
                var response = await _httpClient.PostAsJsonAsync("api/Auth/verify-reset-code", verifyDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                var requestDto = new { Email = email };
                var response = await _httpClient.PostAsJsonAsync("api/Auth/forgot-password", requestDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email, string code, string newPassword)
        {
            try
            {
                var resetDto = new { Email = email, Code = code, NewPassword = newPassword };
                var response = await _httpClient.PostAsJsonAsync("api/Auth/reset-password", resetDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<(bool IsVerified, string Status)> CheckEmailVerificationStatusAsync(string email)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Auth/check-verification-status?email={Uri.EscapeDataString(email)}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content);
                    
                    bool isVerified = result.TryGetProperty("isVerified", out var verifiedProp) && verifiedProp.GetBoolean();
                    string status = result.TryGetProperty("status", out var statusProp) ? statusProp.GetString() ?? "Unknown" : "Unknown";
                    
                    return (isVerified, status);
                }
                
                return (false, "Error");
            }
            catch (Exception)
            {
                return (false, "ConnectionError");
            }
        }

        public async Task<bool> LogoutUserAsync()
        {
            await Task.Delay(1);
            _currentUser = null;
            _authToken = null;
            SecureStorage.Default.Remove("auth_token");
            return true;
        }

        public async Task<string?> GetAuthTokenAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                _authToken = await SecureStorage.Default.GetAsync("auth_token");
            }
            return _authToken;
        }

        // Profile Methods
        public async Task<UserProfile?> GetUserProfileAsync(string userId)
        {
            await Task.Delay(1);
            return _userProfiles.FirstOrDefault(p => p.UserId == userId);
        }

        public async Task<bool> UpdateUserProfileAsync(UserProfile profile)
        {
            await Task.Delay(1);
            var existingProfile = _userProfiles.FirstOrDefault(p => p.UserId == profile.UserId);
            if (existingProfile == null)
            {
                profile.UpdatedAt = DateTime.Now;
                _userProfiles.Add(profile);
                return true;
            }

            var index = _userProfiles.IndexOf(existingProfile);
            profile.UpdatedAt = DateTime.Now;
            _userProfiles[index] = profile;
            return true;
        }

        // Preferences Methods
        public async Task<UserPreferences?> GetUserPreferencesAsync(string userId)
        {
            await Task.Delay(1);
            return _userPreferences.FirstOrDefault(p => p.UserId == userId);
        }

        public async Task<bool> UpdateUserPreferencesAsync(UserPreferences preferences)
        {
            await Task.Delay(1);
            var existingPreferences = _userPreferences.FirstOrDefault(p => p.UserId == preferences.UserId);
            if (existingPreferences == null)
            {
                preferences.UpdatedAt = DateTime.Now;
                _userPreferences.Add(preferences);
                return true;
            }

            var index = _userPreferences.IndexOf(existingPreferences);
            preferences.UpdatedAt = DateTime.Now;
            _userPreferences[index] = preferences;
            return true;
        }

        public async Task<bool> SubmitParentConsentAsync(string studentEmail, string parentName, string parentEmail)
        {
            try
            {
                var model = new { StudentEmail = studentEmail, ParentName = parentName, ParentEmail = parentEmail };
                var response = await _httpClient.PostAsJsonAsync("api/Auth/submit-parent", model);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
