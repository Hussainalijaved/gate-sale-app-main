using GateSale.Models;

namespace GateSale.Services.Interfaces
{
    public interface IOrderTrackingService
    {
        Task<OrderTrackingInfo?> GetOrderTrackingAsync(Guid orderId);
        Task<LockerInfo?> GetLockerStatusAsync(Guid orderId);
    }
}
