using GateSale.Models;
using GateSale.Services.Interfaces;
using System.Net.Http.Json;

namespace GateSale.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private static readonly List<Category> _categories = new();
        private static readonly List<SellingCategory> _sellingCategories = new();
        private static readonly List<CategoryStats> _categoryStats = new();
        private static bool _isInitialized = false;

        // Cache for API-fetched categories
        private static List<CategoryApiDto>? _apiCategories = null;
        private static List<SubCategoryApiDto>? _apiSubCategories = null;
        private static DateTime _lastApiFetch = DateTime.MinValue;
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

        public CategoryService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("GateSaleAPI");
            if (!_isInitialized)
            {
                InitializeData();
                _isInitialized = true;
            }
        }

        private void InitializeData()
        {
            // Initialize main categories
            _categories.AddRange(new List<Category>
            {
                new Category
                {
                    Id = "textbooks",
                    Name = "Textbooks",
                    DisplayName = "Textbooks",
                    Description = "Academic textbooks for all subjects and grades",
                    IconUrl = "/images/icons/books.png",
                    IconEmoji = "📚",
                    Color = "#3B82F6",
                    IsActive = true,
                    SortOrder = 1
                },
                new Category
                {
                    Id = "electronics",
                    Name = "Electronics",
                    DisplayName = "Electronics",
                    Description = "Calculators, laptops, tablets, and other electronic devices",
                    IconUrl = "/images/icons/electronics.png",
                    IconEmoji = "💻",
                    Color = "#10B981",
                    IsActive = true,
                    SortOrder = 2
                },
                new Category
                {
                    Id = "clothing",
                    Name = "Clothing",
                    DisplayName = "Clothing",
                    Description = "School uniforms, casual wear, and accessories",
                    IconUrl = "/images/icons/clothing.png",
                    IconEmoji = "👕",
                    Color = "#F59E0B",
                    IsActive = true,
                    SortOrder = 3
                },
                new Category
                {
                    Id = "school-supplies",
                    Name = "School Supplies",
                    DisplayName = "School Supplies",
                    Description = "Notebooks, pens, backpacks, and other school essentials",
                    IconUrl = "/images/icons/supplies.png",
                    IconEmoji = "✏️",
                    Color = "#EF4444",
                    IsActive = true,
                    SortOrder = 4
                },
                new Category
                {
                    Id = "sports",
                    Name = "Sports",
                    DisplayName = "Sports & Recreation",
                    Description = "Sports equipment, gear, and recreational items",
                    IconUrl = "/images/icons/sports.png",
                    IconEmoji = "⚽",
                    Color = "#8B5CF6",
                    IsActive = true,
                    SortOrder = 5
                },
                new Category
                {
                    Id = "other",
                    Name = "Other",
                    DisplayName = "Other",
                    Description = "Miscellaneous items that don't fit other categories",
                    IconUrl = "/images/icons/other.png",
                    IconEmoji = "📦",
                    Color = "#6B7280",
                    IsActive = true,
                    SortOrder = 6
                }
            });

            // Initialize selling categories (from WhatAreYouSelling.razor)
            _sellingCategories.AddRange(new List<SellingCategory>
            {
                new SellingCategory
                {
                    Id = "academic",
                    Name = "Academic",
                    DisplayName = "Academic / School Supplies",
                    IconEmoji = "📚",
                    Color = "#3B82F6",
                    IsActive = true,
                    IsPopular = true,
                    SortOrder = 1,
                    RequiredFields = new List<string> { "title", "condition" },
                    OptionalFields = new List<string> { "brand", "edition" },
                    SubCategories = new List<SubCategory>
                    {
                        new SubCategory { Id = "textbooks", Name = "textbooks", DisplayName = "Textbooks", IsActive = true, SortOrder = 1 },
                        new SubCategory { Id = "stationery", Name = "stationery", DisplayName = "Stationery & Supplies", IsActive = true, SortOrder = 2 },
                        new SubCategory { Id = "study_guides", Name = "study_guides", DisplayName = "Study Guides & Exam Papers", IsActive = true, SortOrder = 3 },
                        new SubCategory { Id = "summary_notes", Name = "summary_notes", DisplayName = "Exam Summary Notes", IsActive = true, SortOrder = 4 },
                        new SubCategory { Id = "calculators", Name = "calculators", DisplayName = "Calculators & Math Sets", IsActive = true, SortOrder = 5 },
                        new SubCategory { Id = "school_bags", Name = "school_bags", DisplayName = "School Bags", IsActive = true, SortOrder = 6 },
                        new SubCategory { Id = "academic_other", Name = "academic_other", DisplayName = "Other", IsActive = true, SortOrder = 7 }
                    }
                },
                new SellingCategory
                {
                    Id = "uniform",
                    Name = "Uniform",
                    DisplayName = "Uniform & Clothing",
                    IconEmoji = "👕",
                    Color = "#F59E0B",
                    IsActive = true,
                    IsPopular = false,
                    SortOrder = 2,
                    RequiredFields = new List<string> { "size", "condition", "type" },
                    OptionalFields = new List<string> { "brand", "color", "material" },
                    SubCategories = new List<SubCategory>
                    {
                        new SubCategory { Id = "school_uniforms", Name = "school_uniforms", DisplayName = "School Uniforms", IsActive = true, SortOrder = 1 },
                        new SubCategory { Id = "sportswear", Name = "sportswear", DisplayName = "Sportswear & Team Kits", IsActive = true, SortOrder = 2 },
                        new SubCategory { Id = "shoes", Name = "shoes", DisplayName = "Shoes & Accessories", IsActive = true, SortOrder = 3 },
                        new SubCategory { Id = "uniform_other", Name = "uniform_other", DisplayName = "Other", IsActive = true, SortOrder = 4 }
                    }
                },
                new SellingCategory
                {
                    Id = "tech",
                    Name = "Tech",
                    DisplayName = "Tech & Electronics",
                    IconEmoji = "💻",
                    Color = "#10B981",
                    IsActive = true,
                    IsPopular = true,
                    SortOrder = 3,
                    RequiredFields = new List<string> { "brand", "model", "condition" },
                    OptionalFields = new List<string> { "warranty", "accessories", "specifications" },
                    SubCategories = new List<SubCategory>
                    {
                        new SubCategory { Id = "phones", Name = "phones", DisplayName = "Phones & Tablets", IsActive = true, SortOrder = 1 },
                        new SubCategory { Id = "headphones", Name = "headphones", DisplayName = "Headphones & Earbuds", IsActive = true, SortOrder = 2 },
                        new SubCategory { Id = "laptops", Name = "laptops", DisplayName = "Laptops & Accessories", IsActive = true, SortOrder = 3 },
                        new SubCategory { Id = "chargers", Name = "chargers", DisplayName = "Chargers & Power Banks", IsActive = true, SortOrder = 4 },
                        new SubCategory { Id = "smartwatches", Name = "smartwatches", DisplayName = "Smartwatches", IsActive = true, SortOrder = 5 },
                        new SubCategory { Id = "tech_other", Name = "tech_other", DisplayName = "Other", IsActive = true, SortOrder = 6 }
                    }
                },
                new SellingCategory
                {
                    Id = "toys",
                    Name = "Toys",
                    DisplayName = "Toys, Games & Fun",
                    IconEmoji = "🎮",
                    Color = "#8B5CF6",
                    IsActive = true,
                    IsPopular = false,
                    SortOrder = 4,
                    RequiredFields = new List<string> { "description", "condition" },
                    OptionalFields = new List<string> { "brand", "age_range" },
                    SubCategories = new List<SubCategory>
                    {
                        new SubCategory { Id = "board_games", Name = "board_games", DisplayName = "Board Games", IsActive = true, SortOrder = 1 },
                        new SubCategory { Id = "puzzles", Name = "puzzles", DisplayName = "Puzzles", IsActive = true, SortOrder = 2 },
                        new SubCategory { Id = "toys", Name = "toys", DisplayName = "Toys & Figurines", IsActive = true, SortOrder = 3 },
                        new SubCategory { Id = "trading_cards", Name = "trading_cards", DisplayName = "Trading Cards", IsActive = true, SortOrder = 4 },
                        new SubCategory { Id = "toys_other", Name = "toys_other", DisplayName = "Other", IsActive = true, SortOrder = 5 }
                    }
                },
                new SellingCategory
                {
                    Id = "sports",
                    Name = "Sports",
                    DisplayName = "Sports & Outdoors",
                    IconEmoji = "⚽",
                    Color = "#EF4444",
                    IsActive = true,
                    IsPopular = false,
                    SortOrder = 5,
                    RequiredFields = new List<string> { "description", "condition" },
                    OptionalFields = new List<string> { "brand", "size" },
                    SubCategories = new List<SubCategory>
                    {
                        new SubCategory { Id = "cricket", Name = "cricket", DisplayName = "Cricket Gear", IsActive = true, SortOrder = 1 },
                        new SubCategory { Id = "rugby", Name = "rugby", DisplayName = "Rugby Gear", IsActive = true, SortOrder = 2 },
                        new SubCategory { Id = "hockey", Name = "hockey", DisplayName = "Hockey Gear", IsActive = true, SortOrder = 3 },
                        new SubCategory { Id = "soccer", Name = "soccer", DisplayName = "Soccer Gear", IsActive = true, SortOrder = 4 },
                        new SubCategory { Id = "tennis", Name = "tennis", DisplayName = "Tennis Gear", IsActive = true, SortOrder = 5 },
                        new SubCategory { Id = "netball", Name = "netball", DisplayName = "Netball Gear", IsActive = true, SortOrder = 6 },
                        new SubCategory { Id = "athletics", Name = "athletics", DisplayName = "Athletics Gear", IsActive = true, SortOrder = 7 },
                        new SubCategory { Id = "swimming", Name = "swimming", DisplayName = "Swimming Gear", IsActive = true, SortOrder = 8 },
                        new SubCategory { Id = "golf", Name = "golf", DisplayName = "Golf Gear", IsActive = true, SortOrder = 9 },
                        new SubCategory { Id = "other_sport", Name = "other_sport", DisplayName = "Other Sport Gear", IsActive = true, SortOrder = 10 },
                        new SubCategory { Id = "sports_bags", Name = "sports_bags", DisplayName = "Sports Bags", IsActive = true, SortOrder = 11 },
                        new SubCategory { Id = "bicycles", Name = "bicycles", DisplayName = "Bicycles & Skates", IsActive = true, SortOrder = 12 },
                        new SubCategory { Id = "water_bottles", Name = "water_bottles", DisplayName = "Water Bottles & Accessories", IsActive = true, SortOrder = 13 },
                        new SubCategory { Id = "sports_other", Name = "sports_other", DisplayName = "Other", IsActive = true, SortOrder = 14 }
                    }
                },
                new SellingCategory
                {
                    Id = "arts",
                    Name = "Arts",
                    DisplayName = "Arts & Creativity",
                    IconEmoji = "🎨",
                    Color = "#EC4899",
                    IsActive = true,
                    IsPopular = false,
                    SortOrder = 6,
                    RequiredFields = new List<string> { "description", "condition" },
                    OptionalFields = new List<string> { "brand", "medium" },
                    SubCategories = new List<SubCategory>
                    {
                        new SubCategory { Id = "art_supplies", Name = "art_supplies", DisplayName = "Art Supplies", IsActive = true, SortOrder = 1 },
                        new SubCategory { Id = "craft_kits", Name = "craft_kits", DisplayName = "Craft Kits", IsActive = true, SortOrder = 2 },
                        new SubCategory { Id = "instruments", Name = "instruments", DisplayName = "Instruments & Sheet Music", IsActive = true, SortOrder = 3 },
                        new SubCategory { Id = "sketchbooks", Name = "sketchbooks", DisplayName = "Sketchbooks & Portfolios", IsActive = true, SortOrder = 4 }
                    }
                },
                new SellingCategory
                {
                    Id = "miscellaneous",
                    Name = "Miscellaneous",
                    DisplayName = "Miscellaneous",
                    IconEmoji = "📦",
                    Color = "#6B7280",
                    IsActive = true,
                    IsPopular = false,
                    SortOrder = 7,
                    RequiredFields = new List<string> { "description", "condition" },
                    OptionalFields = new List<string> { "brand", "specifications" },
                    SubCategories = new List<SubCategory>
                    {
                        new SubCategory { Id = "locker_decor", Name = "locker_decor", DisplayName = "Locker Décor", IsActive = true, SortOrder = 1 },
                        new SubCategory { Id = "event_tickets", Name = "event_tickets", DisplayName = "Event Tickets", IsActive = true, SortOrder = 2 },
                        new SubCategory { Id = "diy_projects", Name = "diy_projects", DisplayName = "DIY Projects", IsActive = true, SortOrder = 3 },
                        new SubCategory { Id = "not_listed", Name = "not_listed", DisplayName = "Other / Not Listed", IsActive = true, SortOrder = 4 }
                    }
                }
            });

            // Initialize category statistics
            _categoryStats.AddRange(new List<CategoryStats>
            {
                new CategoryStats
                {
                    CategoryId = "textbooks",
                    CategoryName = "Textbooks",
                    ProductCount = 45,
                    ActiveListings = 32,
                    SoldItems = 13,
                    AveragePrice = 42.50m,
                    TotalValue = 1912.50m
                },
                new CategoryStats
                {
                    CategoryId = "electronics",
                    CategoryName = "Electronics",
                    ProductCount = 28,
                    ActiveListings = 21,
                    SoldItems = 7,
                    AveragePrice = 89.99m,
                    TotalValue = 2519.72m
                },
                new CategoryStats
                {
                    CategoryId = "clothing",
                    CategoryName = "Clothing",
                    ProductCount = 15,
                    ActiveListings = 12,
                    SoldItems = 3,
                    AveragePrice = 25.00m,
                    TotalValue = 375.00m
                },
                new CategoryStats
                {
                    CategoryId = "other",
                    CategoryName = "Other",
                    ProductCount = 8,
                    ActiveListings = 6,
                    SoldItems = 2,
                    AveragePrice = 15.50m,
                    TotalValue = 124.00m
                }
            });
        }

        // Category Management Methods
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            await Task.Delay(1);
            return _categories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToList();
        }

        public async Task<Category?> GetCategoryByIdAsync(string categoryId)
        {
            await Task.Delay(1);
            return _categories.FirstOrDefault(c => c.Id == categoryId && c.IsActive);
        }

        public async Task<Category?> GetCategoryByNameAsync(string categoryName)
        {
            await Task.Delay(1);
            return _categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase) && c.IsActive);
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            await Task.Delay(1);
            if (string.IsNullOrEmpty(category.Id))
                category.Id = Guid.NewGuid().ToString();

            category.CreatedAt = DateTime.Now;
            category.UpdatedAt = DateTime.Now;
            _categories.Add(category);
            return true;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            await Task.Delay(1);
            var existingCategory = _categories.FirstOrDefault(c => c.Id == category.Id);
            if (existingCategory == null) return false;

            var index = _categories.IndexOf(existingCategory);
            category.UpdatedAt = DateTime.Now;
            _categories[index] = category;
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(string categoryId)
        {
            await Task.Delay(1);
            var category = _categories.FirstOrDefault(c => c.Id == categoryId);
            if (category == null) return false;

            category.IsActive = false;
            category.UpdatedAt = DateTime.Now;
            return true;
        }

        // Category Statistics Methods
        public async Task<int> GetProductCountByCategoryAsync(string categoryId)
        {
            await Task.Delay(1);
            var stats = _categoryStats.FirstOrDefault(s => s.CategoryId == categoryId);
            return stats?.ProductCount ?? 0;
        }

        public async Task<List<CategoryStats>> GetCategoryStatsAsync()
        {
            await Task.Delay(1);
            return _categoryStats.ToList();
        }

        // Selling Categories Methods
        public async Task<List<SellingCategory>> GetSellingCategoriesAsync()
        {
            await Task.Delay(1);
            return _sellingCategories.Where(sc => sc.IsActive).OrderBy(sc => sc.SortOrder).ToList();
        }

        public async Task<bool> IsCategoryValidForSellingAsync(string categoryName)
        {
            await Task.Delay(1);
            return _sellingCategories.Any(sc => sc.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase) && sc.IsActive);
        }

        // ==========================================
        // BACKEND API INTEGRATION METHODS
        // ==========================================

        /// <summary>
        /// Fetches all categories with subcategories from backend API
        /// GET api/category
        /// </summary>
        public async Task<List<CategoryApiDto>> GetCategoriesFromApiAsync()
        {
            // Return cached data if available and not expired
            if (_apiCategories != null && DateTime.Now - _lastApiFetch < CacheExpiry)
            {
                return _apiCategories;
            }

            try
            {
                var response = await _httpClient.GetAsync("api/category");
                
                if (response.IsSuccessStatusCode)
                {
                    var categories = await response.Content.ReadFromJsonAsync<List<CategoryApiDto>>();
                    if (categories != null)
                    {
                        _apiCategories = categories;
                        _lastApiFetch = DateTime.Now;
                        return categories;
                    }
                }
                
                // Log error for debugging
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"GetCategoriesFromApiAsync failed: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCategoriesFromApiAsync exception: {ex.Message}");
            }

            // Return cached data if available (even if expired) or empty list as fallback
            return _apiCategories ?? new List<CategoryApiDto>();
        }

        public async Task<List<SubCategoryApiDto>> GetSubCategoriesFromApiAsync()
        {
            // Return cached data if available and not expired
            if (_apiSubCategories != null && DateTime.Now - _lastApiFetch < CacheExpiry)
            {
                return _apiSubCategories;
            }

            try
            {
                var response = await _httpClient.GetAsync("api/category/subcategories");
                
                if (response.IsSuccessStatusCode)
                {
                    var subCategories = await response.Content.ReadFromJsonAsync<List<SubCategoryApiDto>>();
                    if (subCategories != null)
                    {
                        _apiSubCategories = subCategories;
                        _lastApiFetch = DateTime.Now;
                        return subCategories;
                    }
                }
                
                // Log error for debugging
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"GetSubCategoriesFromApiAsync failed: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSubCategoriesFromApiAsync exception: {ex.Message}");
            }

            // Return cached data if available (even if expired) or empty list as fallback
            return _apiSubCategories ?? new List<SubCategoryApiDto>();
        }

        public async Task<List<Category>> GetMergedCategoriesAsync()
        {
            var apiCategories = await GetCategoriesFromApiAsync();
            var mergedList = new List<Category>();

            foreach (var apiCat in apiCategories)
            {
                // Try to find a matching local category to get rich data (Icon, Color, etc.)
                var localCat = _categories.FirstOrDefault(c => 
                    c.Name.Equals(apiCat.Name, StringComparison.OrdinalIgnoreCase) || 
                    c.Id.Equals(apiCat.Name, StringComparison.OrdinalIgnoreCase)); // Check ID too just in case

                if (localCat != null)
                {
                    // Use local category but ensure ID/Name matches API if needed
                    // For now, we trust the local rich data
                    mergedList.Add(localCat);
                }
                else
                {
                    // Create a new category with default styling
                    mergedList.Add(new Category
                    {
                        Id = apiCat.Name.ToLower().Replace(" ", "-"), // Generate a slug-like ID
                        Name = apiCat.Name,
                        DisplayName = apiCat.Name,
                        Description = "",
                        IconUrl = "/images/icons/other.png", // Default icon
                        IconEmoji = "📦", // Default emoji
                        Color = "#6B7280", // Default gray color
                        IsActive = true,
                        SortOrder = 99
                    });
                }
            }

            // If API failed or returned empty, fallback to local categories
            if (!mergedList.Any())
            {
                return _categories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToList();
            }

            return mergedList.OrderBy(c => c.SortOrder).ToList();
        }

        /// <summary>
        /// Clears the cached API data (useful for refresh)
        /// </summary>
        public void ClearApiCache()
        {
            _apiCategories = null;
            _apiSubCategories = null;
            _lastApiFetch = DateTime.MinValue;
        }
    }
}
