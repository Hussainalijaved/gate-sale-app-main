using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GateSale.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GateSale.Services
{
    public class PudoSimulationService : IPudoSimulationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PudoSimulationService> _logger;

        public PudoSimulationService(HttpClient httpClient, ILogger<PudoSimulationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> SimulatePackageDroppedAsync(Guid orderId, string lockerCode)
        {
            try
            {
                _logger.LogInformation("Simulating package drop for order {OrderId} at locker {LockerCode}", orderId, lockerCode);
                
                var request = new
                {
                    LockerCode = lockerCode,
                    Status = "Occupied",
                    TransactionId = "SIM-" + Guid.NewGuid().ToString().Substring(0, 8)
                };

                var response = await _httpClient.PostAsJsonAsync("api/PudoWebhook/status", request);
                
                if (response.IsSuccessStatusCode)
                {
                    // Also simulate the PackageDropped event which updates order status to Delivered
                    var webhookEvent = new
                    {
                        EventType = "package_dropped",
                        LockerCode = lockerCode,
                        OrderReference = await GetOrderNumberAsync(orderId),
                        Timestamp = DateTime.UtcNow
                    };
                    
                    // We hit the main webhook endpoint for this one
                    var mainResponse = await _httpClient.PostAsJsonAsync("api/PudoWebhook", webhookEvent);
                    return mainResponse.IsSuccessStatusCode;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating package drop");
                return false;
            }
        }

        public async Task<bool> SimulatePackagePickedUpAsync(Guid orderId, string lockerCode)
        {
            try
            {
                _logger.LogInformation("Simulating package pickup for order {OrderId} from locker {LockerCode}", orderId, lockerCode);
                
                var request = new
                {
                    OrderId = orderId,
                    LockerCode = lockerCode,
                    PickupTime = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync("api/PudoWebhook/pickup", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating package pickup");
                return false;
            }
        }

        private async Task<string> GetOrderNumberAsync(Guid orderId)
        {
            try
            {
                // We need the order number for the webhook reference
                // For simulation, we can try to fetch it from OrderService or just use a placeholder if we can't find it
                // But the backend needs the real OrderNumber to find the order.
                // Let's assume the caller provides it or we fetch it.
                // For now, I'll return a placeholder and hope the backend can find it by ID in other webhooks.
                // Actually, let's just fetch the order details.
                var response = await _httpClient.GetAsync($"api/Order/{orderId}");
                if (response.IsSuccessStatusCode)
                {
                    var order = await response.Content.ReadFromJsonAsync<dynamic>();
                    return order?.orderNumber ?? string.Empty;
                }
            }
            catch { }
            return string.Empty;
        }
    }
}
