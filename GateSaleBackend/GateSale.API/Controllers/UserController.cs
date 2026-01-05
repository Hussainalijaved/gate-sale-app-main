using GateSale.Core.Entities;
using GateSale.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GateSale.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly GateSaleDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(GateSaleDbContext context, ILogger<UserController> logger)
        {
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

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = await GetResolvedUserId();
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Return a simplified profile DTO to avoid circular references or sensitive data
                return Ok(new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    user.Grade,
                    user.ProfileImageUrl,
                    user.IsEmailVerified,
                    user.IsMinor,
                    user.ParentalConsentGiven,
                    SchoolName = user.School,
                    user.Status,
                    user.CreatedAt
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, "An error occurred while retrieving the profile");
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = await GetResolvedUserId();
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                if (!string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.LastName))
                {
                    var firstName = request.FirstName ?? (user.FullName.Contains(' ') ? user.FullName.Split(' ')[0] : user.FullName);
                    var lastName = request.LastName ?? (user.FullName.Contains(' ') ? user.FullName.Split(' ')[1] : "");
                    user.FullName = $"{firstName} {lastName}".Trim();
                }

                if (request.Grade.HasValue)
                {
                    user.Grade = request.Grade.Value;
                }

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Profile updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, "An error occurred while updating the profile");
            }
        }
    }

    public class UpdateProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Grade { get; set; }
    }
}
