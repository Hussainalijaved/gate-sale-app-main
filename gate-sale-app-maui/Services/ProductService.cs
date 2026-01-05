using GateSale.Models;
using GateSale.Services.Interfaces;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GateSale.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IHttpClientFactory httpClientFactory, IUserService userService, ILogger<ProductService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("GateSaleAPI");
            _userService = userService;
            _logger = logger;
        }


        public void ForceReinitialize()
        {
            // No longer needed with API integration
        }

        public string GetProductDetailIdFromTrendingId(string trendingId)
        {
            // Map trending item IDs to product detail IDs
            return trendingId switch
            {
                "trending1" => "book",        // Calculus AP -> Book details
                "trending2" => "calculator",  // TI-84 Calculator -> Calculator details
                "trending3" => "headphone",   // Wireless Headphones -> Headphone details
                          _ => "book" // Default fallback
            };
        }

        // Product Management Methods
        public async Task<List<Product>> GetAllProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ProductListApiDto>("api/Product");
                if (response?.Products == null) return new List<Product>();

                return response.Products.Select(MapToProduct).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all products");
                return new List<Product>();
            }
        }

        public async Task<Product?> GetProductByIdAsync(string id)
        {
            try
            {
                var productDto = await _httpClient.GetFromJsonAsync<ProductApiDto>($"api/Product/{id}");
                return productDto != null ? MapToProduct(productDto) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product by ID: {ProductId}", id);
                return null;
            }
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ProductListApiDto>($"api/Product?category={category}");
                if (response?.Products == null) return new List<Product>();

                return response.Products.Select(MapToProduct).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products by category: {Category}", category);
                return new List<Product>();
            }
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ProductListApiDto>($"api/Product/search?q={searchTerm}");
                if (response?.Products == null) return new List<Product>();

                return response.Products.Select(MapToProduct).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with term: {SearchTerm}", searchTerm);
                return new List<Product>();
            }
        }

        public async Task<List<Product>> GetMyProductsAsync()
        {
            try
            {
                // Add auth token
                var token = await _userService.GetAuthTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No auth token available for GetMyProducts");
                    return new List<Product>();
                }

                var response = await _httpClient.GetFromJsonAsync<ProductListApiDto>("api/Product/my");
                if (response?.Products == null) return new List<Product>();

                return response.Products.Select(MapToProduct).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user's products");
                return new List<Product>();
            }
        }

        public async Task<Guid?> CreateProductAsync(ProductCreationData data)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(data.Title), "Title");
                content.Add(new StringContent(data.Description), "Description");
                content.Add(new StringContent(data.PriceValue.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price");
                content.Add(new StringContent(data.GetPrimaryCategory()), "Category");
                content.Add(new StringContent(MapConditionToEnum(data.Condition).ToString()), "Condition");
                
                if (!string.IsNullOrEmpty(data.SubCategory))
                    content.Add(new StringContent(data.SubCategory), "SubCategory");

                foreach (var photo in data.Photos)
                {
                    if (!string.IsNullOrEmpty(photo.Base64Data))
                    {
                        var bytes = Convert.FromBase64String(photo.Base64Data);
                        var byteContent = new ByteArrayContent(bytes);
                        byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(photo.ContentType);
                        content.Add(byteContent, "Images", photo.Name);
                    }
                }

                var token = await _userService.GetAuthTokenAsync();

                var response = await _httpClient.PostAsync("api/Product", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (result.TryGetProperty("id", out var idProp))
                    {
                        return idProp.GetGuid();
                    }
                    if (result.TryGetProperty("Id", out var idProp2))
                    {
                        return idProp2.GetGuid();
                    }
                    
                    _logger.LogWarning("Product created successfully but 'id' property was missing in response: {Response}", result.GetRawText());
                    throw new Exception("Product created but ID was not returned by the server.");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error creating product: Status {StatusCode}, Error: {Error}", response.StatusCode, error);
                    // Throw an exception with the error message so the UI can show it
                    throw new Exception(error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw; // Rethrow to let the UI handle it
            }
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            // This is now deprecated in favor of CreateProductAsync(ProductCreationData)
            _logger.LogWarning("AddProductAsync(Product) is deprecated. Use CreateProductAsync(ProductCreationData) instead.");
            return false;
        }

        private int MapConditionToEnum(string condition)
        {
            return condition switch
            {
                "Brand New" => 0,
                "New" => 0,
                "Like New" => 1,
                "Excellent" => 1,
                "Good" => 2,
                "Fair" => 3,
                "Poor" => 4,
                _ => 2 // Default to Good
            };
        }


        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                // Add auth token
                var token = await _userService.GetAuthTokenAsync();

                // Create update DTO matching backend expectations
                var updateDto = new
                {
                    Title = product.Name,
                    Description = product.Description,
                    Price = decimal.Parse(product.Price.Replace("R", "").Replace("$", "").Replace(",", "").Trim()),
                    Category = product.Category,
                    Condition = MapConditionToEnum(product.Condition)
                };

                var response = await _httpClient.PutAsJsonAsync($"api/Product/{product.Id}", updateDto);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error updating product: Status {StatusCode}, Error: {Error}", response.StatusCode, error);
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", product.Id);
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            try
            {
                // Add auth token
                var token = await _userService.GetAuthTokenAsync();

                var response = await _httpClient.DeleteAsync($"api/Product/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error deleting product: Status {StatusCode}, Error: {Error}", response.StatusCode, error);
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return false;
            }
        }

        // Product Details Methods
        public async Task<ProductDetail?> GetProductDetailByIdAsync(string id)
        {
            try
            {
                var productDto = await _httpClient.GetFromJsonAsync<ProductApiDto>($"api/Product/{id}");
                return productDto != null ? MapToProductDetail(productDto) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product detail by ID: {ProductId}", id);
                return null;
            }
        }

        public async Task<List<ProductDetail>> GetAllProductDetailsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ProductListApiDto>("api/Product");
                if (response?.Products == null) return new List<ProductDetail>();

                return response.Products.Select(MapToProductDetail).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all product details");
                return new List<ProductDetail>();
            }
        }

        // Trending and Recent Methods
        public async Task<List<TrendingItem>> GetTrendingItemsAsync()
        {
            try
            {
                // For now, trending items are just the latest products
                var response = await _httpClient.GetFromJsonAsync<ProductListApiDto>("api/Product?pageSize=10&sortBy=CreatedAt&sortOrder=desc");
                if (response?.Products == null) return new List<TrendingItem>();

                return response.Products.Select(MapToTrendingItem).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trending items");
                return new List<TrendingItem>();
            }
        }

        public void RefreshTrendingItems()
        {
            // Handled by API calls
        }

        public async Task<List<RecentListing>> GetRecentListingsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ProductListApiDto>("api/Product?pageSize=10&sortBy=CreatedAt&sortOrder=desc");
                if (response?.Products == null) return new List<RecentListing>();

                return response.Products.Select(MapToRecentListing).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent listings");
                return new List<RecentListing>();
            }
        }

        public async Task<List<TrendingItem>> GetFilteredTrendingItemsAsync(ProductFilter filter)
        {
            try
            {
                var queryParams = new List<string>();
                if (filter.MinPrice.HasValue) queryParams.Add($"minPrice={filter.MinPrice}");
                if (filter.MaxPrice.HasValue) queryParams.Add($"maxPrice={filter.MaxPrice}");
                if (!string.IsNullOrEmpty(filter.Category)) queryParams.Add($"category={filter.Category}");
                if (!string.IsNullOrEmpty(filter.SearchTerm)) queryParams.Add($"searchTerm={filter.SearchTerm}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetFromJsonAsync<ProductListApiDto>($"api/Product{queryString}");
                
                if (response?.Products == null) return new List<TrendingItem>();
                return response.Products.Select(MapToTrendingItem).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered trending items");
                return new List<TrendingItem>();
            }
        }

        // Statistics Methods
        public async Task<int> GetProductCountAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ProductListApiDto>("api/Product?pageSize=1");
                return response?.TotalCount ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product count");
                return 0;
            }
        }

        public async Task<int> GetProductCountByCategoryAsync(string category)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ProductListApiDto>($"api/Product?category={category}&pageSize=1");
                return response?.TotalCount ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product count by category");
                return 0;
            }
        }

        public async Task AddUserProductAsync(TrendingItem trendingItem, ProductDetail productDetail)
        {
            _logger.LogWarning("AddUserProductAsync is deprecated. Use AddProductAsync instead.");
            await Task.CompletedTask;
        }

        // Mapping Methods
        private Product MapToProduct(ProductApiDto dto)
        {
            return new Product
            {
                Id = dto.Id.ToString(),
                Name = dto.Title,
                Price = $"R {dto.Price:F2}",
                Category = dto.Category,
                Description = dto.Description,
                ImageUrl = dto.Images.OrderBy(i => i.Order).FirstOrDefault()?.ImageUrl ?? "/images/placeholder.jpg",
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.CreatedAt,
                SellerId = dto.SellerId.ToString(),
                Condition = dto.Condition,
                IsActive = dto.Status == "Active"
            };
        }

        private ProductDetail MapToProductDetail(ProductApiDto dto)
        {
            return new ProductDetail
            {
                Id = dto.Id.ToString(),
                Title = dto.Title,
                Category = dto.Category,
                CurrentPrice = $"R {dto.Price:F2}",
                ImageUrl = dto.Images.OrderBy(i => i.Order).FirstOrDefault()?.ImageUrl ?? "/images/placeholder.jpg",
                ImageUrls = dto.Images.OrderBy(i => i.Order).Select(i => i.ImageUrl).ToList(),
                Description = dto.Description,
                Condition = dto.Condition,
                SellerId = dto.SellerId.ToString(),
                SellerName = dto.SellerName,
                SellerLocation = dto.SellerSchool,
                CreatedAt = dto.CreatedAt,
                IsVerified = true, // Defaulting for now
                Rating = "5.0" // Defaulting for now
            };
        }

        private TrendingItem MapToTrendingItem(ProductApiDto dto)
        {
            return new TrendingItem
            {
                Id = dto.Id.ToString(),
                Title = dto.Title,
                Price = $"R {dto.Price:F2}",
                PriceValue = dto.Price,
                Details = dto.SellerSchool,
                ImageUrl = dto.Images.OrderBy(i => i.Order).FirstOrDefault()?.ImageUrl ?? "/images/placeholder.jpg",
                Condition = dto.Condition,
                CreatedAt = dto.CreatedAt,
                BadgeText = "New",
                BadgeType = "Hot"
            };
        }

        private RecentListing MapToRecentListing(ProductApiDto dto)
        {
            return new RecentListing
            {
                Id = dto.Id.ToString(),
                Title = dto.Title,
                Price = $"R {dto.Price:F2}",
                Details = dto.Description.Length > 50 ? dto.Description.Substring(0, 47) + "..." : dto.Description,
                ImageUrl = dto.Images.OrderBy(i => i.Order).FirstOrDefault()?.ImageUrl ?? "/images/placeholder.jpg",
                SellerId = dto.SellerId.ToString(),
                Category = dto.Category,
                Condition = dto.Condition,
                CreatedAt = dto.CreatedAt,
                TimeAgo = GetTimeAgo(dto.CreatedAt),
                IsNew = (DateTime.UtcNow - dto.CreatedAt).TotalDays < 2
            };
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;
            if (span.TotalDays > 365) return $"{(int)(span.TotalDays / 365)}y ago";
            if (span.TotalDays > 30) return $"{(int)(span.TotalDays / 30)}m ago";
            if (span.TotalDays > 1) return $"{(int)span.TotalDays}d ago";
            if (span.TotalHours > 1) return $"{(int)span.TotalHours}h ago";
            if (span.TotalMinutes > 1) return $"{(int)span.TotalMinutes}m ago";
            return "Just now";
        }

    }
}
