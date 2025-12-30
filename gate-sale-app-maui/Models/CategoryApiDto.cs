namespace GateSale.Models
{
    /// <summary>
    /// DTO for category data returned from backend API (GET api/category)
    /// </summary>
    public class CategoryApiDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public List<SubCategoryApiDto> SubCategories { get; set; } = new();
    }

    /// <summary>
    /// DTO for subcategory data returned from backend API (GET api/category/subcategories)
    /// </summary>
    public class SubCategoryApiDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public Guid CategoryId { get; set; }
    }
}
