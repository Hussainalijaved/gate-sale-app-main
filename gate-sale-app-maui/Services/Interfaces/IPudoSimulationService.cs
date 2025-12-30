using System;
using System.Threading.Tasks;

namespace GateSale.Services.Interfaces
{
    public interface IPudoSimulationService
    {
        Task<bool> SimulatePackageDroppedAsync(Guid orderId, string lockerCode);
        Task<bool> SimulatePackagePickedUpAsync(Guid orderId, string lockerCode);
    }
}
