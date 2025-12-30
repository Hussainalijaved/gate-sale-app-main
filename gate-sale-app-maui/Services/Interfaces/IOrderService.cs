using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GateSale.Models;

namespace GateSale.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderSummaryDto?> CreateOrderAsync(CreateOrderRequest request);
        Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId);
        Task<List<OrderSummaryDto>> GetBuyerOrdersAsync();
        Task<List<OrderSummaryDto>> GetSellerOrdersAsync();
        Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, string? notes = null);
        Task<bool> ProcessShipmentAsync(Guid orderId, string trackingNumber, string reference);
        Task<bool> ApproveOrderAsync(Guid orderId);
        Task<bool> CancelOrderAsync(Guid orderId, string reason, string? description = null);
    }
}
