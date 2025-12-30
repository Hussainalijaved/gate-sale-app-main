using System;

namespace GateSale.Models
{
    public class InitiatePaymentRequest
    {
        public Guid OrderId { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }

    public class PaymentResultDto
    {
        public string PaymentUrl { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }

    public class RefundPaymentRequest
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class RefundResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? RefundReference { get; set; }
        public decimal? Amount { get; set; }
    }
}
