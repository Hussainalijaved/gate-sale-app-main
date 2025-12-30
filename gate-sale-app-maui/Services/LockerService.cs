using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GateSale.Models;
using GateSale.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GateSale.Services
{
    public class LockerService : ILockerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LockerService> _logger;

        public LockerService(HttpClient httpClient, ILogger<LockerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<LockerDto>> GetNearbyLockersAsync(double latitude, double longitude, double radius = 10)
        {
            try
            {
                var url = $"api/Locker/nearby?latitude={latitude}&longitude={longitude}&radius={radius}";
                var lockers = await _httpClient.GetFromJsonAsync<List<LockerDto>>(url);
                return lockers ?? new List<LockerDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nearby lockers");
                return new List<LockerDto>();
            }
        }

        public async Task<LockerDto?> GetLockerByCodeAsync(string lockerCode)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<LockerDto>($"api/Locker/{lockerCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting locker {LockerCode}", lockerCode);
                return null;
            }
        }
    }
}
