namespace GateSale.Models
{
    public class Category
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";
        public string IconUrl { get; set; } = "";
        public string IconEmoji { get; set; } = "";
        public string Color { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public string? ParentCategoryId { get; set; }
        public List<Category> SubCategories { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class SellingCategory
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string IconEmoji { get; set; } = "";
        public string Color { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public bool IsPopular { get; set; } = false;
        public int SortOrder { get; set; } = 0;
        public List<string> RequiredFields { get; set; } = new();
        public List<string> OptionalFields { get; set; } = new();
        public List<SubCategory> SubCategories { get; set; } = new();
    }
    
    public class SubCategory
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }

    public class CategoryStats
    {
        public string CategoryId { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public int ProductCount { get; set; } = 0;
        public int ActiveListings { get; set; } = 0;
        public int SoldItems { get; set; } = 0;
        public decimal AveragePrice { get; set; } = 0;
        public decimal TotalValue { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
