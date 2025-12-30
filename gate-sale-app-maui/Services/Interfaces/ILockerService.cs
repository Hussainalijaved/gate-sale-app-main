using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GateSale.Models;

namespace GateSale.Services.Interfaces
{
    public interface ILockerService
    {
        Task<List<LockerDto>> GetNearbyLockersAsync(double latitude, double longitude, double radius = 10);
        Task<LockerDto?> GetLockerByCodeAsync(string lockerCode);
    }
}
