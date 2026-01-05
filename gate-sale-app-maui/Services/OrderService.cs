using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GateSale.Models;
using GateSale.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GateSale.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            HttpClient httpClient,
            ILogger<OrderService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<OrderSummaryDto?> CreateOrderAsync(CreateOrderRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Order", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<OrderSummaryDto>();
                }
                
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error creating order: {Error}", error);
                
                // Try to parse error message if it's JSON
                try 
                {
                    var errorObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(error);
                    if (errorObj.TryGetProperty("message", out var msg)) error = msg.GetString();
                    else if (errorObj.TryGetProperty("Message", out var msg2)) error = msg2.GetString();
                }
                catch { /* Not JSON or no message property */ }

                throw new Exception(error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception creating order");
                throw;
            }
        }

        public async Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<OrderDetailDto>($"api/Order/{orderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                return null;
            }
        }

        public async Task<List<OrderSummaryDto>> GetBuyerOrdersAsync()
        {
            try
            {
                var orders = await _httpClient.GetFromJsonAsync<List<OrderSummaryDto>>("api/Order/buyer");
                return orders ?? new List<OrderSummaryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting buyer orders");
                return new List<OrderSummaryDto>();
            }
        }

        public async Task<List<OrderSummaryDto>> GetSellerOrdersAsync()
        {
            try
            {
                var orders = await _httpClient.GetFromJsonAsync<List<OrderSummaryDto>>("api/Order/seller");
                return orders ?? new List<OrderSummaryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seller orders");
                return new List<OrderSummaryDto>();
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, string? notes = null)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Order/{orderId}/status", new UpdateOrderStatusRequest
                {
                    Status = status,
                    Notes = notes
                });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> ProcessShipmentAsync(Guid orderId, string trackingNumber, string reference)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Order/{orderId}/shipment", new ProcessShipmentRequest
                {
                    PudoTrackingNumber = trackingNumber,
                    PudoShipmentReference = reference
                });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shipment for {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> ApproveOrderAsync(Guid orderId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/Order/{orderId}/approve", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving order {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> CancelOrderAsync(Guid orderId, string reason, string? description = null)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Order/{orderId}/cancel", new CancelOrderRequest
                {
                    Reason = reason,
                    Description = description
                });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                return false;
            }
        }
    }
}
