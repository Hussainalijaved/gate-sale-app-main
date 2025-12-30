using GateSale.Models;
using GateSale.Services.Interfaces;
using System.Net.Http.Json;

namespace GateSale.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;

        public PaymentService(IHttpClientFactory httpClientFactory, IUserService userService)
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

        public async Task<PaymentResultDto?> InitiatePaymentAsync(InitiatePaymentRequest request)
        {
            try
            {
                await SetAuthHeader();
                var response = await _httpClient.PostAsJsonAsync("api/Payment/initiate", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PaymentResultDto>();
                }
                
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error initiating payment: {response.StatusCode} - {error}");
                return new PaymentResultDto { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception initiating payment: {ex.Message}");
                return new PaymentResultDto { Success = false, Message = ex.Message };
            }
        }

        public async Task<RefundResultDto?> RefundPaymentAsync(RefundPaymentRequest request)
        {
            try
            {
                await SetAuthHeader();
                var response = await _httpClient.PostAsJsonAsync("api/Payment/refund", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RefundResultDto>();
                }
                
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error processing refund: {response.StatusCode} - {error}");
                return new RefundResultDto { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception processing refund: {ex.Message}");
                return new RefundResultDto { Success = false, Message = ex.Message };
            }
        }
    }
}
