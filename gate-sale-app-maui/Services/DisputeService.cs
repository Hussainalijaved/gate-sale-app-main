using GateSale.Models;
using GateSale.Services.Interfaces;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace GateSale.Services
{
    public class DisputeService : IDisputeService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;

        public DisputeService(IHttpClientFactory httpClientFactory, IUserService userService)
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

        public async Task<DisputeDetailDto?> CreateDisputeAsync(Guid orderId, CreateDisputeRequest request)
        {
            try
            {
                await SetAuthHeader();
                var response = await _httpClient.PostAsJsonAsync($"api/Dispute/order/{orderId}", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DisputeDetailDto>();
                }
                
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error creating dispute: {response.StatusCode} - {error}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception creating dispute: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UploadEvidenceAsync(Guid disputeId, byte[] fileData, string fileName, string contentType, string? caption)
        {
            try
            {
                await SetAuthHeader();
                using var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(fileData);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                content.Add(fileContent, "File", fileName);
                
                if (!string.IsNullOrEmpty(caption))
                {
                    content.Add(new StringContent(caption), "Caption");
                }

                var response = await _httpClient.PostAsync($"api/Dispute/{disputeId}/evidence", content);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error uploading evidence: {response.StatusCode} - {error}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception uploading evidence: {ex.Message}");
                return false;
            }
        }

        public async Task<DisputeDetailDto?> GetDisputeByOrderIdAsync(Guid orderId)
        {
            try
            {
                await SetAuthHeader();
                var response = await _httpClient.GetAsync($"api/Dispute/order/{orderId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DisputeDetailDto>();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> RequestReturnAsync(Guid orderId, bool isSellerPaying)
        {
            try
            {
                await SetAuthHeader();
                var request = new RequestReturnRequest { IsSellerPayingForReturn = isSellerPaying };
                var response = await _httpClient.PostAsJsonAsync($"api/Dispute/order/{orderId}/return", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> MarkReturnShippedAsync(Guid orderId, string trackingNumber, string? reference)
        {
            try
            {
                await SetAuthHeader();
                var request = new MarkReturnShippedRequest 
                { 
                    ReturnTrackingNumber = trackingNumber,
                    ReturnShipmentReference = reference
                };
                var response = await _httpClient.PostAsJsonAsync($"api/Dispute/order/{orderId}/return/shipped", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> MarkReturnDeliveredAsync(Guid orderId)
        {
            try
            {
                await SetAuthHeader();
                var response = await _httpClient.PostAsync($"api/Dispute/order/{orderId}/return/delivered", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> MarkReturnCollectedAsync(Guid orderId)
        {
            try
            {
                await SetAuthHeader();
                var response = await _httpClient.PostAsync($"api/Dispute/order/{orderId}/return/collected", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> WaiveReturnAsync(Guid orderId)
        {
            try
            {
                await SetAuthHeader();
                var response = await _httpClient.PostAsync($"api/Dispute/order/{orderId}/waive-return", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ReviewDisputeAsync(Guid disputeId, bool isApproved, string notes)
        {
            try
            {
                await SetAuthHeader();
                var request = new ReviewDisputeRequest { IsApproved = isApproved, Notes = notes };
                var response = await _httpClient.PostAsJsonAsync($"api/Dispute/{disputeId}/review", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
