namespace GateSale.Models
{
    public class Product
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Price { get; set; } = "";
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string OriginalPrice { get; set; } = "";
        public string DiscountBadge { get; set; } = "";
        public List<string> Features { get; set; } = new();
        public string Condition { get; set; } = "";
        public string Grade { get; set; } = "";
        public bool IsVerified { get; set; } = false;
        public string Rating { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string SellerId { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public class ProductDetail
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string CurrentPrice { get; set; } = "";
        public string? OriginalPrice { get; set; }
        public string? DiscountBadge { get; set; }
        public string ImageUrl { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Features { get; set; } = new();
        public string Condition { get; set; } = "";
        public string Grade { get; set; } = "";
        public bool IsVerified { get; set; } = false;
        public string Rating { get; set; } = "";
        public string SellerId { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<string> ImageUrls { get; set; } = new();

        // Additional properties for product details page
        public string DistanceFromUser { get; set; } = "";
        public string DeliveryFee { get; set; } = "";
        public string SellerName { get; set; } = "";
        public string SellerLocation { get; set; } = "";
    }

    public class TrendingItem
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Price { get; set; } = "";
        public string Details { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string BadgeText { get; set; } = "";
        public string BadgeType { get; set; } = "";
        public decimal PriceValue { get; set; }
        public string Condition { get; set; } = "";
        public string Grade { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ViewCount { get; set; } = 0;
        public int FavoriteCount { get; set; } = 0;
    }

    public class RecentListing
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Price { get; set; } = "";
        public string Details { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public bool IsVerified { get; set; } = false;
        public bool HasSameCourse { get; set; } = false;
        public string Rating { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string SellerId { get; set; } = "";
        public string Category { get; set; } = "";
        public string Condition { get; set; } = "";
        public string Grade { get; set; } = "";

        // Additional properties for recent listings
        public string TimeAgo { get; set; } = "";
        public string Location { get; set; } = "";
        public bool IsNew { get; set; } = false;
    }
}
