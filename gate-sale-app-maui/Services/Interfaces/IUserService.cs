using GateSale.Models;

namespace GateSale.Services.Interfaces
{
    public interface IUserService
    {
        // User Management
        Task<User?> GetCurrentUserAsync();
        Task<User?> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> SetCurrentUserAsync(string userId);

        // Authentication State
        Task<bool> IsUserLoggedInAsync();
        Task<(bool Success, string Message)> LoginUserAsync(string email, string password);
        Task<(bool Success, string Message)> RegisterUserAsync(User user, string password);
        Task<bool> VerifyEmailAsync(string email, string code);
        Task<bool> VerifyResetCodeAsync(string email, string code);
        Task<(bool IsVerified, string Status)> CheckEmailVerificationStatusAsync(string email);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string code, string newPassword);
        Task<bool> LogoutUserAsync();

        // User Profile
        Task<UserProfile?> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(UserProfile profile);
        Task<string?> GetAuthTokenAsync();

        // User Preferences
        Task<UserPreferences?> GetUserPreferencesAsync(string userId);
        Task<bool> UpdateUserPreferencesAsync(UserPreferences preferences);
        Task<bool> SubmitParentConsentAsync(string studentEmail, string parentName, string parentEmail);
    }
}
