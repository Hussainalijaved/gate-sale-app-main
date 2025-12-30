namespace GateSale.Models
{
    public class User
    {
        public string Id { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string SchoolId { get; set; } = "";
        public string Grade { get; set; } = "";
        public DateTime DateOfBirth { get; set; } = DateTime.Now.AddYears(-15);
        public string ProfileImageUrl { get; set; } = "";
        public bool IsVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
    }

    public class UserProfile
    {
        public string UserId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Bio { get; set; } = "";
        public string ProfileImageUrl { get; set; } = "";
        public string Grade { get; set; } = "";
        public string SchoolId { get; set; } = "";
        public List<string> Interests { get; set; } = new();
        public UserStats Stats { get; set; } = new();
        public UserSettings Settings { get; set; } = new();
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class UserStats
    {
        public int ItemsSold { get; set; } = 0;
        public int ItemsBought { get; set; } = 0;
        public int FavoriteItems { get; set; } = 0;
        public decimal TotalEarnings { get; set; } = 0;
        public decimal TotalSpent { get; set; } = 0;
        public double AverageRating { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;
    }

    public class UserSettings
    {
        public bool NotificationsEnabled { get; set; } = true;
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public bool ShowOnlineStatus { get; set; } = true;
        public string PreferredLanguage { get; set; } = "en";
        public string Currency { get; set; } = "USD";
    }

    public class UserPreferences
    {
        public string UserId { get; set; } = "";
        public List<string> PreferredCategories { get; set; } = new();
        public List<string> PreferredGrades { get; set; } = new();
        public decimal? MaxBudget { get; set; }
        public decimal? MinBudget { get; set; }
        public List<string> PreferredConditions { get; set; } = new();
        public bool OnlyVerifiedSellers { get; set; } = false;
        public bool OnlySameCourse { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
