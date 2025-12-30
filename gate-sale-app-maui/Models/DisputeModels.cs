using System;
using System.Collections.Generic;

namespace GateSale.Models
{
    public enum OrderStatus
    {
        PaidAwaitingShipment = 0,
        InTransit = 1,
        Delivered = 2,
        Collected = 3,
        BuyerApproved = 4,
        AwaitingPayment = 5,
        DisputeInProgress = 10,
        DisputeApproved = 11,
        DisputeRejected = 12,
        AwaitingReturn = 20,
        ReturnInTransit = 21,
        ReturnDelivered = 22,
        ReturnCollected = 23,
        CancelledBySeller = 30,
        AwaitingRefund = 40,
        Refunded = 41,
        AwaitingPayout = 42,
        Completed = 50
    }

    public enum DisputeStatus
    {
        Open = 0,
        InReview = 1,
        Approved = 2,
        Rejected = 3,
        AwaitingReturn = 4,
        ReturnReceived = 5,
        Resolved = 6,
        Closed = 7
    }

    public enum DisputeReason
    {
        ItemNotAsDescribed = 0,
        ItemDamaged = 1,
        ItemNotWorking = 2,
        WrongItemReceived = 3,
        PoorCondition = 4,
        Other = 5
    }

    public class CreateDisputeRequest
    {
        public DisputeReason ReasonCode { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ReviewDisputeRequest
    {
        public bool IsApproved { get; set; }
        public string? Notes { get; set; }
    }

    public class RequestReturnRequest
    {
        public bool IsSellerPayingForReturn { get; set; }
    }

    public class MarkReturnShippedRequest
    {
        public string ReturnTrackingNumber { get; set; } = string.Empty;
        public string? ReturnShipmentReference { get; set; }
    }

    public class DisputeDetailDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public DisputeReason ReasonCode { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DisputeStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public bool? IsApproved { get; set; }
        public string? AdminNotes { get; set; }
        public bool IsReturnRequested { get; set; }
        public bool IsReturnPaidBySeller { get; set; }
        public bool IsReturnCompleted { get; set; }
        public List<DisputeEvidenceDto> Evidence { get; set; } = new List<DisputeEvidenceDto>();
    }

    public class DisputeEvidenceDto
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public string? FileType { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
