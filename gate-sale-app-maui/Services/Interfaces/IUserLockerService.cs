using GateSale.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GateSale.Services.Interfaces
{
    public interface IUserLockerService
    {
        Task<IEnumerable<UserLockerDto>> GetFavoriteLockersAsync();
        Task<UserLockerDto?> GetDefaultLockerAsync();
        Task<IEnumerable<UserLockerDto>> GetSellerDropoffLockersAsync();
        Task<UserLockerDto?> AddFavoriteLockerAsync(Guid? lockerId, string? lockerCode);
        Task<bool> RemoveFavoriteLockerAsync(string lockerCode);
        Task<UserLockerDto?> SetDefaultLockerAsync(Guid? lockerId, string? lockerCode);
        Task<UserLockerDto?> SetSellerDropoffLockerAsync(Guid? lockerId, string? lockerCode);
    }
}
