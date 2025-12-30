using System;
using System.Collections.Generic;

namespace GateSale.Models
{
    public class ProductApiDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Keywords { get; set; }
        public Guid SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public string SellerSchool { get; set; } = string.Empty;
        public List<ProductImageApiDto> Images { get; set; } = new();
        public string? SubCategory { get; set; }
    }

    public class ProductImageApiDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Order { get; set; }
    }

    public class ProductListApiDto
    {
        public List<ProductApiDto> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ProductFilterApiDto
    {
        public string? Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Condition { get; set; }
        public string? School { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }

    public class ProductSearchApiDto
    {
        public string? Q { get; set; }
        public string? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int Limit { get; set; } = 20;
        public int Offset { get; set; } = 0;
    }

    public class AuthResponseApiDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string? UserId { get; set; }
        public UserProfileApiDto User { get; set; } = new();
    }

    public class UserProfileApiDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool ParentalConsentGiven { get; set; }
        public bool IsProfileComplete { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
