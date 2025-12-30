using GateSale.Models;

namespace GateSale.Services.Interfaces
{
    public interface IProductService
    {
        // Product Management
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(string id);
        Task<List<Product>> GetProductsByCategoryAsync(string category);
        Task<List<Product>> SearchProductsAsync(string searchTerm);
        Task<List<Product>> GetMyProductsAsync();
        Task<bool> AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(string id);

        // Product Details
        Task<ProductDetail?> GetProductDetailByIdAsync(string id);
        Task<List<ProductDetail>> GetAllProductDetailsAsync();

        // Trending and Recent
        Task<List<TrendingItem>> GetTrendingItemsAsync();
        Task<List<RecentListing>> GetRecentListingsAsync();
        Task<List<TrendingItem>> GetFilteredTrendingItemsAsync(ProductFilter filter);
        string GetProductDetailIdFromTrendingId(string trendingId);

        // Product Statistics
        Task<int> GetProductCountAsync();
        Task<int> GetProductCountByCategoryAsync(string category);

        // User Product Creation
        Task<Guid?> CreateProductAsync(ProductCreationData data);
        Task AddUserProductAsync(TrendingItem trendingItem, ProductDetail productDetail);
    }

    public class ProductFilter
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string> Conditions { get; set; } = new();
        public List<string> Grades { get; set; } = new();
        public string? Category { get; set; }
        public string? SearchTerm { get; set; }
    }
}
