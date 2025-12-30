using GateSale.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GateSale.Services.Interfaces
{
    public interface IDisputeService
    {
        Task<DisputeDetailDto?> CreateDisputeAsync(Guid orderId, CreateDisputeRequest request);
        Task<bool> UploadEvidenceAsync(Guid disputeId, byte[] fileData, string fileName, string contentType, string? caption);
        Task<DisputeDetailDto?> GetDisputeByOrderIdAsync(Guid orderId);
        Task<bool> RequestReturnAsync(Guid orderId, bool isSellerPaying);
        Task<bool> MarkReturnShippedAsync(Guid orderId, string trackingNumber, string? reference);
        Task<bool> MarkReturnDeliveredAsync(Guid orderId);
        Task<bool> MarkReturnCollectedAsync(Guid orderId);
        Task<bool> WaiveReturnAsync(Guid orderId);
        Task<bool> ReviewDisputeAsync(Guid disputeId, bool isApproved, string notes);
    }
}
