using GateSale.Models;
using GateSale.Components.Pages.Selling;
using static GateSale.Components.Pages.Selling.AddPhotosOrVideos;

namespace GateSale.Services
{
    public class ProductCreationService
    {
        private ProductCreationData _currentProduct = new();
        
        // Events for data changes
        public event Action? OnDataChanged;
        
        public ProductCreationData GetCurrentProduct()
        {
            return _currentProduct;
        }
        
        public void SetCategories(List<string> categories)
        {
            _currentProduct.Categories = categories.ToList();
            NotifyDataChanged();
        }
        
        public void SetSubCategory(string subCategory)
        {
            _currentProduct.SubCategory = subCategory;
            NotifyDataChanged();
        }
        
        public void SetPhotos(List<MediaFile> photos)
        {
            _currentProduct.Photos = photos.ToList();
            NotifyDataChanged();
        }
        

        
        public void SetTitleAndDescription(string title, string description)
        {
            _currentProduct.Title = title;
            _currentProduct.Description = description;
            NotifyDataChanged();
        }
        
        public void SetPrice(string price, decimal priceValue)
        {
            _currentProduct.Price = price;
            _currentProduct.PriceValue = priceValue;
            NotifyDataChanged();
        }
        
        public void SetAdditionalDetails(string grade, string building, string condition)
        {
            _currentProduct.Grade = grade;
            _currentProduct.Building = building;
            _currentProduct.Condition = condition;
            NotifyDataChanged();
        }
        
        public bool IsReadyForReview()
        {
            return _currentProduct.IsValid();
        }
        
        public void ResetProduct()
        {
            _currentProduct.Reset();
            NotifyDataChanged();
        }
        
        public TrendingItem CreateTrendingItem()
        {
            if (!_currentProduct.IsValid())
                throw new InvalidOperationException("Product data is not complete");
                
            var id = $"user_product_{Guid.NewGuid().ToString("N")[..8]}";
            _currentProduct.Id = id;
            
            return new TrendingItem
            {
                Id = id,
                Title = _currentProduct.Title,
                Price = _currentProduct.GetFormattedPrice(),
                Details = _currentProduct.GetLocationDetails(),
                ImageUrl = _currentProduct.GetMainImageUrl(),
                BadgeText = "New",
                BadgeType = "Hot",
                PriceValue = _currentProduct.PriceValue,
                Condition = _currentProduct.Condition,
                Grade = _currentProduct.Grade,
                ViewCount = 0,
                FavoriteCount = 0
            };
        }
        
        public ProductDetail CreateProductDetail()
        {
            if (!_currentProduct.IsValid())
                throw new InvalidOperationException("Product data is not complete");
                
            var id = _currentProduct.Id;
            if (string.IsNullOrEmpty(id))
            {
                id = $"user_product_{Guid.NewGuid().ToString("N")[..8]}";
                _currentProduct.Id = id;
            }
            
            return new ProductDetail
            {
                Id = id,
                Title = _currentProduct.Title,
                Category = MapCategoryToDisplayCategory(_currentProduct.GetPrimaryCategory()),
                CurrentPrice = _currentProduct.GetFormattedPrice(),
                OriginalPrice = "", // No original price for new listings
                ImageUrl = _currentProduct.GetMainImageUrl(),
                ImageUrls = _currentProduct.Photos.Select(p => p.PreviewUrl).ToList(),
                Description = _currentProduct.Description,
                Features = _currentProduct.Title.Split(' ')
                    .Where(word => word.Length > 2)
                    .Select(word => $"• {word}")
                    .ToList(),
                DistanceFromUser = "0.1 miles", // Default for new listings
                DeliveryFee = "Free", // Default
                SellerName = "You",
                SellerLocation = _currentProduct.GetLocationDetails(),
                Grade = _currentProduct.Grade,
                IsVerified = true,
                Rating = "New",
                SellerId = "current_user"
            };
        }
        
        private void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
        }

        private string MapCategoryToDisplayCategory(string sellingCategory)
        {
            return sellingCategory.ToLower() switch
            {
                "books" => "Textbooks",
                "electronics" => "Electronics",
                "clothing" => "Clothing",
                "other" => "School Supplies",
                _ => "School Supplies"
            };
        }
    }
}
