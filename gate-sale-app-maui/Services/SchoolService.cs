using System.Net.Http;
using System.Net.Http.Json;
using GateSale.Models;
using GateSale.Services.Interfaces;

namespace GateSale.Services
{
    public class SchoolService : ISchoolService
    {
        private static readonly List<School> _schools = new();
        private static School? _currentSchool = null;
        private static bool _isInitialized = false;
        private readonly HttpClient _httpClient;

        public SchoolService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            if (!_isInitialized)
            {
                InitializeData();
                _isInitialized = true;
            }
        }

        private void InitializeData()
        {
            var lincolnHigh = new School
            {
                Id = "lincoln-high",
                Name = "Lincoln High School",
                Address = "123 Education Street",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                LogoUrl = "/images/lin.png",
                IsVerified = true,
                IsActive = true,
                ActiveStudentCount = 2847, // From Marketplace.razor
                Buildings = new List<string> { "Building A", "Building B", "Building C", "Main Building" },
                SupportedGrades = new List<Grade>
                {
                    new Grade { Id = "grade9", Name = "Grade 9", DisplayName = "Grade 9", Level = 9, IsActive = true },
                    new Grade { Id = "grade10", Name = "Grade 10", DisplayName = "Grade 10", Level = 10, IsActive = true },
                    new Grade { Id = "grade11", Name = "Grade 11", DisplayName = "Grade 11", Level = 11, IsActive = true },
                    new Grade { Id = "grade12", Name = "Grade 12", DisplayName = "Grade 12", Level = 12, IsActive = true }
                },
                CreatedAt = DateTime.Now.AddYears(-5),
                Settings = new SchoolSettings
                {
                    AllowCrossCampusTrading = true,
                    RequireVerification = false,
                    AllowGuestAccess = false,
                    RestrictedCategories = new List<string>(),
                    MaxTransactionAmount = 1000,
                    RequireParentalConsent = true
                }
            };

            var springfieldHigh = new School
            {
                Id = "springfield-high",
                Name = "Springfield High School",
                Address = "456 Academic Avenue",
                City = "Springfield",
                State = "IL",
                ZipCode = "62702",
                LogoUrl = "/images/springfield-logo.png",
                IsVerified = true,
                IsActive = true,
                ActiveStudentCount = 1923,
                Buildings = new List<string> { "North Building", "South Building", "Science Wing" },
                SupportedGrades = new List<Grade>
                {
                    new Grade { Id = "grade9", Name = "Grade 9", DisplayName = "Grade 9", Level = 9, IsActive = true },
                    new Grade { Id = "grade10", Name = "Grade 10", DisplayName = "Grade 10", Level = 10, IsActive = true },
                    new Grade { Id = "grade11", Name = "Grade 11", DisplayName = "Grade 11", Level = 11, IsActive = true },
                    new Grade { Id = "grade12", Name = "Grade 12", DisplayName = "Grade 12", Level = 12, IsActive = true }
                },
                CreatedAt = DateTime.Now.AddYears(-3),
                Settings = new SchoolSettings
                {
                    AllowCrossCampusTrading = false,
                    RequireVerification = true,
                    AllowGuestAccess = false,
                    RestrictedCategories = new List<string> { "Electronics" },
                    MaxTransactionAmount = 500,
                    RequireParentalConsent = true
                }
            };

            _schools.AddRange(new[] { lincolnHigh, springfieldHigh });
            
            // Set Lincoln High as current school (from Marketplace.razor)
            _currentSchool = lincolnHigh;
        }

        // School Management Methods
        public async Task<List<School>> GetAllSchoolsAsync()
        {
            await Task.Delay(1);
            return _schools.Where(s => s.IsActive).ToList();
        }

        public async Task<School?> GetSchoolByIdAsync(string schoolId)
        {
            await Task.Delay(1);
            return _schools.FirstOrDefault(s => s.Id == schoolId && s.IsActive);
        }

        public async Task<School?> GetCurrentSchoolAsync()
        {
            await Task.Delay(1);
            return _currentSchool;
        }

        public async Task<bool> SetCurrentSchoolAsync(string schoolId)
        {
            await Task.Delay(1);
            var school = await GetSchoolByIdAsync(schoolId);
            if (school == null) return false;

            _currentSchool = school;
            return true;
        }

        public async Task<School?> GetSchoolByNameAsync(string name)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Auth/check-school?name={Uri.EscapeDataString(name)}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<School>();
                }
                
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"School check failed: {response.StatusCode} - {error}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"School check error: {ex.Message}");
                return null;
            }
        }

        // School Information Methods
        public async Task<int> GetActiveStudentCountAsync(string schoolId)
        {
            await Task.Delay(1);
            var school = await GetSchoolByIdAsync(schoolId);
            return school?.ActiveStudentCount ?? 0;
        }

        public async Task<List<string>> GetSchoolBuildingsAsync(string schoolId)
        {
            await Task.Delay(1);
            var school = await GetSchoolByIdAsync(schoolId);
            return school?.Buildings ?? new List<string>();
        }

        public async Task<List<Grade>> GetSchoolGradesAsync(string schoolId)
        {
            await Task.Delay(1);
            var school = await GetSchoolByIdAsync(schoolId);
            return school?.SupportedGrades ?? new List<Grade>();
        }

        // School Verification Methods
        public async Task<bool> IsSchoolVerifiedAsync(string schoolId)
        {
            await Task.Delay(1);
            var school = await GetSchoolByIdAsync(schoolId);
            return school?.IsVerified ?? false;
        }

        public async Task<bool> VerifySchoolAsync(string schoolId)
        {
            await Task.Delay(1);
            var school = _schools.FirstOrDefault(s => s.Id == schoolId);
            if (school == null) return false;

            school.IsVerified = true;
            school.UpdatedAt = DateTime.Now;
            return true;
        }

        public async Task<ApiResponse<bool>> RequestSchoolAsync(string schoolName, string city, string requesterEmail, string? contactPerson)
        {
            try
            {
                var requestDto = new
                {
                    SchoolName = schoolName,
                    City = city,
                    RequesterEmail = requesterEmail,
                    ContactPerson = contactPerson
                };

                var response = await _httpClient.PostAsJsonAsync("api/Auth/request-school", requestDto);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool> { Success = true, Message = "School request submitted successfully." };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(errorContent);
                    if (errorObj.TryGetProperty("message", out var msg))
                    {
                        return new ApiResponse<bool> { Success = false, Message = msg.GetString() ?? "Request failed." };
                    }
                }
                catch { }

                return new ApiResponse<bool> { Success = false, Message = "Failed to submit school request." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Connection error: {ex.Message}" };
            }
        }
    }
}
