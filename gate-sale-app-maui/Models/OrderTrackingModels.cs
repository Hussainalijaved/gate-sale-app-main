using System;
using System.Collections.Generic;

namespace GateSale.Models
{
    public enum LockerStatus
    {
        Available = 0,
        Occupied = 1,
        Maintenance = 2,
        OutOfService = 3
    }

    public class OrderTrackingInfo
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public LockerInfo? Locker { get; set; }
        public List<OrderTrackingEvent> Events { get; set; } = new List<OrderTrackingEvent>();
    }

    public class LockerInfo
    {
        public string LockerCode { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
        public LockerStatus Status { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class OrderTrackingEvent
    {
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Location { get; set; }
    }
}
