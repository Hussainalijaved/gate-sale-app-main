namespace GateSale.Models
{
    public class School
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public string ZipCode { get; set; } = "";
        public string LogoUrl { get; set; } = "";
        public bool IsVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int ActiveStudentCount { get; set; } = 0;
        public List<string> Buildings { get; set; } = new();
        public List<Grade> SupportedGrades { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public SchoolSettings Settings { get; set; } = new();
    }

    public class Grade
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public int Level { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    public class SchoolSettings
    {
        public bool AllowCrossCampusTrading { get; set; } = true;
        public bool RequireVerification { get; set; } = false;
        public bool AllowGuestAccess { get; set; } = false;
        public List<string> RestrictedCategories { get; set; } = new();
        public decimal MaxTransactionAmount { get; set; } = 1000;
        public bool RequireParentalConsent { get; set; } = true;
    }
}
