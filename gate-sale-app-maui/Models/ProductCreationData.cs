using GateSale.Components.Pages.Selling;
using static GateSale.Components.Pages.Selling.AddPhotosOrVideos;

namespace GateSale.Models
{
    public class ProductCreationData
    {
        public List<string> Categories { get; set; } = new();
        public string SubCategory { get; set; } = "";
        public List<MediaFile> Photos { get; set; } = new();
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Price { get; set; } = "";
        public decimal PriceValue { get; set; } = 0;
        
        // Additional properties for marketplace integration
        public string Id { get; set; } = "";
        public string Grade { get; set; } = "Grade 12"; // Default
        public string Building { get; set; } = "Building A"; // Default
        public string Condition { get; set; } = "Good"; // Default
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public bool IsValid()
        {
            return Categories.Count > 0 && 
                   !string.IsNullOrWhiteSpace(SubCategory) &&
                   Photos.Count > 0 && 
                   !string.IsNullOrWhiteSpace(Title) && 
                   !string.IsNullOrWhiteSpace(Description) && 
                   PriceValue > 0;
        }
        
        public void Reset()
        {
            Categories.Clear();
            SubCategory = "";
            Photos.Clear();
            Title = "";
            Description = "";
            Price = "";
            PriceValue = 0;
            Id = "";
            CreatedAt = DateTime.Now;
        }
        
        public string GetPrimaryCategory()
        {
            return Categories.FirstOrDefault() ?? "General";
        }
        
        public string GetMainImageUrl()
        {
            return Photos.FirstOrDefault()?.PreviewUrl ?? "/images/placeholder.jpg";
        }
        
        public string GetFormattedPrice()
        {
            return Price.StartsWith("R ") ? Price : $"R {PriceValue:F2}";
        }
        
        public string GetLocationDetails()
        {
            return $"{Grade} • {Building}";
        }
    }
}
