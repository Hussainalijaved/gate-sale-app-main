using System;
using System.Collections.Generic;

namespace GateSale.Models
{

    public class CreateOrderRequest
    {
        public Guid ProductId { get; set; }
        public Guid BuyerLockerId { get; set; }
        public decimal ShippingCost { get; set; }
        public string? PackageSize { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class ProcessShipmentRequest
    {
        public string PudoTrackingNumber { get; set; } = string.Empty;
        public string PudoShipmentReference { get; set; } = string.Empty;
    }

    public class CancelOrderRequest
    {
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class OrderSummaryDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public string? ProductTitle { get; set; }
        public string? ProductImageUrl { get; set; }
        public Guid? SellerLockerId { get; set; }
        public Guid? BuyerLockerId { get; set; }
    }

    public class OrderDetailDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        
        public decimal ItemSubtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AdminFeeAmount { get; set; }
        public decimal SellerPayoutAmount { get; set; }
        
        public Guid ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public string? ProductDescription { get; set; }
        public string? ProductImageUrl { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? SubCategory { get; set; }
        
        public string? PudoTrackingNumber { get; set; }
        public string? PudoShipmentReference { get; set; }
        public string? PackageSize { get; set; }
        
        public LockerDto? BuyerLocker { get; set; }
        public LockerDto? SellerLocker { get; set; }
        
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CollectedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        
        public bool HasDispute { get; set; }
        public DisputeDto? Dispute { get; set; }
    }

    public class LockerDto
    {
        public Guid Id { get; set; }
        public string LockerCode { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class DisputeDto
    {
        public Guid Id { get; set; }
        public int ReasonCode { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Status { get; set; }
        public bool IsReturnRequested { get; set; }
        public bool IsReturnPaidBySeller { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
