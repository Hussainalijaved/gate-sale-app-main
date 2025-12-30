using GateSale.Models;
using GateSale.Services.Interfaces;
using System.Net.Http.Json;

namespace GateSale.Services
{
    public class OrderTrackingService : IOrderTrackingService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;

        public OrderTrackingService(IHttpClientFactory httpClientFactory, IUserService userService)
        {
            _httpClient = httpClientFactory.CreateClient("GateSaleAPI");
            _userService = userService;
        }

        private async Task SetAuthHeader()
        {
            var token = await _userService.GetAuthTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<OrderTrackingInfo?> GetOrderTrackingAsync(Guid orderId)
        {
            try
            {
                await SetAuthHeader();
                var response = await _httpClient.GetAsync($"api/OrderTracking/{orderId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Order Tracking JSON: {json}");
                    return System.Text.Json.JsonSerializer.Deserialize<OrderTrackingInfo>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error getting order tracking: {response.StatusCode} - {error}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting order tracking: {ex.Message}");
                return null;
            }
        }

        public async Task<LockerInfo?> GetLockerStatusAsync(Guid orderId)
        {
            try
            {
                await SetAuthHeader();
                var response = await _httpClient.GetAsync($"api/OrderTracking/{orderId}/locker-status");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<LockerInfo>();
                }

                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error getting locker status: {response.StatusCode} - {error}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting locker status: {ex.Message}");
                return null;
            }
        }
    }
}
