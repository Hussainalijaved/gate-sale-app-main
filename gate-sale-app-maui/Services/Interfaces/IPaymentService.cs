using GateSale.Models;
using System;
using System.Threading.Tasks;

namespace GateSale.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResultDto?> InitiatePaymentAsync(InitiatePaymentRequest request);
        Task<RefundResultDto?> RefundPaymentAsync(RefundPaymentRequest request);
    }
}
