using GateSale.Models;

namespace GateSale.Services.Interfaces
{
    public interface ICategoryService
    {
        // Category Management
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(string categoryId);
        Task<Category?> GetCategoryByNameAsync(string categoryName);
        Task<bool> AddCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(string categoryId);

        // Category Statistics
        Task<int> GetProductCountByCategoryAsync(string categoryId);
        Task<List<CategoryStats>> GetCategoryStatsAsync();

        // Selling Categories
        Task<List<SellingCategory>> GetSellingCategoriesAsync();
        Task<bool> IsCategoryValidForSellingAsync(string categoryName);

        // Backend API Integration
        Task<List<CategoryApiDto>> GetCategoriesFromApiAsync();
        Task<List<SubCategoryApiDto>> GetSubCategoriesFromApiAsync();
        Task<List<Category>> GetMergedCategoriesAsync();
        void ClearApiCache();
    }
}

