using GateSale.Core.Entities;
using GateSale.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GateSale.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserLockerController : ControllerBase
    {
        private readonly IUserLockerService _userLockerService;
        private readonly GateSale.Infrastructure.Data.GateSaleDbContext _context;
        private readonly ILogger<UserLockerController> _logger;

        public UserLockerController(
            IUserLockerService userLockerService,
            GateSale.Infrastructure.Data.GateSaleDbContext context,
            ILogger<UserLockerController> logger)
        {
            _userLockerService = userLockerService;
            _context = context;
            _logger = logger;
        }

        private async Task<Guid> GetResolvedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            // 1. Try to look up by CognitoUserId (most reliable for Cognito tokens)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.CognitoUserId == userIdClaim);
            
            // 2. If not found, try to parse as Guid and look up by Id
            if (user == null && Guid.TryParse(userIdClaim, out var guidId))
            {
                user = await _context.Users.FindAsync(guidId);
            }

            // 3. If still not found, try to look up by Email
            if (user == null)
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                }
            }
            
            if (user == null)
            {
                _logger.LogWarning("User not found in database for claim: {UserIdClaim}", userIdClaim);
                throw new UnauthorizedAccessException("User not found in system");
            }

            return user.Id;
        }

        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavoriteLockers()
        {
            try
            {
                var userId = await GetResolvedUserId();
                var lockers = await _userLockerService.GetUserFavoriteLockers(userId);
                return Ok(lockers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite lockers");
                return StatusCode(500, "An error occurred while retrieving favorite lockers");
            }
        }

        [HttpGet("default")]
        public async Task<IActionResult> GetDefaultLocker()
        {
            try
            {
                var userId = await GetResolvedUserId();
                var locker = await _userLockerService.GetUserDefaultLocker(userId);
                
                if (locker == null)
                {
                    return NotFound("No default locker set");
                }
                
                return Ok(locker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default locker");
                return StatusCode(500, "An error occurred while retrieving default locker");
            }
        }

        [HttpGet("seller/dropoff")]
        public async Task<IActionResult> GetSellerDropoffLockers()
        {
            try
            {
                var sellerId = await GetResolvedUserId();
                var lockers = await _userLockerService.GetSellerDropoffLockers(sellerId);
                return Ok(lockers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seller dropoff lockers");
                return StatusCode(500, "An error occurred while retrieving seller dropoff lockers");
            }
        }

        [HttpPost("favorites")]
        public async Task<IActionResult> AddFavoriteLocker([FromBody] AddLockerRequest request)
        {
            try
            {
                var userId = await GetResolvedUserId();
                UserLocker userLocker;
                
                if (!string.IsNullOrEmpty(request.LockerCode))
                {
                    userLocker = await _userLockerService.AddFavoriteLockerByCode(userId, request.LockerCode);
                }
                else if (request.LockerId.HasValue)
                {
                    userLocker = await _userLockerService.AddFavoriteLocker(userId, request.LockerId.Value);
                }
                else
                {
                    return BadRequest("Either lockerId or lockerCode must be provided");
                }
                
                return Ok(userLocker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding favorite locker");
                return StatusCode(500, "An error occurred while adding favorite locker");
            }
        }

        [HttpDelete("favorites/{lockerCode}")]
        public async Task<IActionResult> RemoveFavoriteLocker(string lockerCode)
        {
            try
            {
                var userId = await GetResolvedUserId();
                var result = await _userLockerService.RemoveFavoriteLockerByCode(userId, lockerCode);
                
                if (!result)
                {
                    return NotFound($"Locker {lockerCode} not found in favorites");
                }
                
                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing favorite locker {LockerCode}", lockerCode);
                return StatusCode(500, "An error occurred while removing favorite locker");
            }
        }

        [HttpPost("default")]
        public async Task<IActionResult> SetDefaultLocker([FromBody] AddLockerRequest request)
        {
            try
            {
                var userId = await GetResolvedUserId();
                UserLocker userLocker;
                
                if (!string.IsNullOrEmpty(request.LockerCode))
                {
                    userLocker = await _userLockerService.SetDefaultLockerByCode(userId, request.LockerCode);
                }
                else if (request.LockerId.HasValue)
                {
                    userLocker = await _userLockerService.SetDefaultLocker(userId, request.LockerId.Value);
                }
                else
                {
                    return BadRequest("Either lockerId or lockerCode must be provided");
                }
                
                return Ok(userLocker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default locker");
                return StatusCode(500, "An error occurred while setting default locker");
            }
        }

        [HttpPost("seller/dropoff")]
        public async Task<IActionResult> SetSellerDropoffLocker([FromBody] AddLockerRequest request)
        {
            try
            {
                var sellerId = await GetResolvedUserId();
                UserLocker userLocker;
                
                if (!string.IsNullOrEmpty(request.LockerCode))
                {
                    userLocker = await _userLockerService.SetSellerDropoffLockerByCode(sellerId, request.LockerCode);
                }
                else if (request.LockerId.HasValue)
                {
                    userLocker = await _userLockerService.SetSellerDropoffLocker(sellerId, request.LockerId.Value);
                }
                else
                {
                    return BadRequest("Either lockerId or lockerCode must be provided");
                }
                
                return Ok(userLocker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting seller dropoff locker");
                return StatusCode(500, "An error occurred while setting seller dropoff locker");
            }
        }
    }

    public class AddLockerRequest
    {
        public Guid? LockerId { get; set; }
        public string? LockerCode { get; set; }
    }
}
