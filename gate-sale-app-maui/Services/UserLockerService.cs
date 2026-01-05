using GateSale.Models;
using GateSale.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GateSale.Services
{
    public class UserLockerService : IUserLockerService
    {
        private readonly HttpClient _httpClient;

        public UserLockerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<UserLockerDto>> GetFavoriteLockersAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<UserLockerDto>>("api/UserLocker/favorites") ?? new List<UserLockerDto>();
            }
            catch
            {
                return new List<UserLockerDto>();
            }
        }

        public async Task<UserLockerDto?> GetDefaultLockerAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/UserLocker/default");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserLockerDto>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<UserLockerDto>> GetSellerDropoffLockersAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<UserLockerDto>>("api/UserLocker/seller/dropoff") ?? new List<UserLockerDto>();
            }
            catch
            {
                return new List<UserLockerDto>();
            }
        }

        public async Task<UserLockerDto?> AddFavoriteLockerAsync(Guid? lockerId, string? lockerCode)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/UserLocker/favorites", new AddLockerRequest { LockerId = lockerId, LockerCode = lockerCode });
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserLockerDto>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> RemoveFavoriteLockerAsync(string lockerCode)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/UserLocker/favorites/{lockerCode}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserLockerDto?> SetDefaultLockerAsync(Guid? lockerId, string? lockerCode)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/UserLocker/default", new AddLockerRequest { LockerId = lockerId, LockerCode = lockerCode });
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserLockerDto>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<UserLockerDto?> SetSellerDropoffLockerAsync(Guid? lockerId, string? lockerCode)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/UserLocker/seller/dropoff", new AddLockerRequest { LockerId = lockerId, LockerCode = lockerCode });
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserLockerDto>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
