using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using GateSale.Core.DTOs;
using GateSale.Core.Entities;
using GateSale.Core.Enums;
using GateSale.Core.Exceptions;
using GateSale.Core.Interfaces;
using GateSale.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace GateSale.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ICognitoService _cognitoService;
        private readonly IEmailService _emailService;
        private readonly IDomainValidationService _domainValidationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly GateSaleDbContext _dbContext;
        
        // Track email resend attempts to prevent abuse
        private static readonly Dictionary<string, (int Count, DateTime LastAttempt)> _resendAttempts = 
            new Dictionary<string, (int Count, DateTime LastAttempt)>();
        private const int MaxResendAttempts = 3;
        private static readonly TimeSpan ResendCooldown = TimeSpan.FromHours(1);

        public AuthController(
            ICognitoService cognitoService,
            IEmailService emailService,
            IDomainValidationService domainValidationService,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            GateSaleDbContext dbContext)
        {
            _cognitoService = cognitoService;
            _emailService = emailService;
            _domainValidationService = domainValidationService;
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            // Validate input
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Extract domain from email
                var email = model.Email.ToLower().Trim();
                var emailParts = email.Split('@');
                
                if (emailParts.Length != 2)
                {
                    return BadRequest(new { message = "Invalid email format" });
                }
                
                var domain = emailParts[1];
                
                // Check if domain is whitelisted
                var isDomainWhitelisted = await _domainValidationService.IsDomainWhitelistedAsync(domain);
                
                if (!isDomainWhitelisted)
                {
                    return BadRequest(new { 
                        message = "Your school is not yet supported", 
                        domain = domain,
                        status = "UnsupportedSchool" 
                    });
                }

                // Check if user already exists in our database
                var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User already exists" });
                }

                AuthResponseDto cognitoResponse;
                
                try 
                {
                    // Register user with Cognito - this will handle email verification automatically
                    cognitoResponse = await _cognitoService.RegisterUserAsync(model);
                }
                catch (ApplicationException ex) when (ex.Message == "An account with this email already exists.")
                {
                    // User exists in Cognito but not in our database - create local record
                    _logger.LogInformation("User exists in Cognito but not in local database. Creating local record.");
                    
                    // Get the Cognito user ID without requiring a login
                    var cognitoUserId = await _cognitoService.GetUserIdByEmailAsync(model.Email);
                    if (string.IsNullOrEmpty(cognitoUserId))
                    {
                        _logger.LogError("User exists in Cognito but could not retrieve UserID for {Email}", model.Email);
                        return BadRequest(new { message = "Account exists but unable to retrieve details. Please contact support." });
                    }
                    
                    cognitoResponse = new AuthResponseDto 
                    { 
                        UserId = cognitoUserId,
                        Token = string.Empty,
                        RefreshToken = string.Empty,
                        ExpiresAt = DateTime.UtcNow,
                        User = new UserProfileDto
                        {
                            Email = model.Email,
                            FullName = model.FullName,
                            School = model.School
                        }
                    };
                }
                
                // Create local user record in our database
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = email,
                    Email = email,
                    FullName = model.FullName,
                    School = model.School,
                    Grade = model.Grade,
                    DateOfBirth = DateTime.SpecifyKind(model.DateOfBirth, DateTimeKind.Utc),
                    IsEmailVerified = false,
                    Status = UserStatus.PendingEmailVerification,
                    IsMinor = IsMinor(model.DateOfBirth),
                    CognitoUserId = cognitoResponse.UserId ?? string.Empty
                };

                _dbContext.Users.Add(user);

                _logger.LogInformation("Creating email verification token for {Email}", email);
                // Generate manual verification token
                var verificationToken = Guid.NewGuid().ToString();
                var emailVerification = new EmailVerification
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    VerificationCode = verificationToken,
                    Type = VerificationType.EmailVerification,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
                _dbContext.EmailVerifications.Add(emailVerification);

                _logger.LogInformation("Saving user and verification token to DB for {Email}", email);
                await _dbContext.SaveChangesAsync();

                // Send verification email manually in the background to avoid blocking the response
                _ = Task.Run(async () =>
                {
                    try
                    {
                        _logger.LogInformation("Sending manual verification email to {Email} (background)", email);
                        var callbackUrl = $"{_configuration["AppSettings:ApiUrl"]}/api/Auth/confirm-email";
                        await _emailService.SendVerificationEmailAsync(email, verificationToken, callbackUrl);
                        _logger.LogInformation("Verification email sent successfully to {Email}", email);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send verification email to {Email} in background", email);
                    }
                });

                return Ok(new { 
                    message = "Registration successful. Please check your email to verify your account.",
                    status = "PendingEmailVerification"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred during registration" });
            }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Confirm signup with Cognito
                var result = await _cognitoService.ConfirmSignUpAsync(model.Email, model.Code);
                if (!result)
                {
                    return BadRequest(new { message = "Email verification failed" });
                }

                // Update our local user record
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.IsEmailVerified = true;
                user.Status = user.IsMinor ? UserStatus.PendingParentalConsent : UserStatus.Active;
                
                await _dbContext.SaveChangesAsync();

                // If the user is not a minor, they are good to go
                if (!user.IsMinor)
                {
                    return Ok(new { 
                        message = "Email verification successful. You can now log in.", 
                        status = "Active" 
                    });
                }

                // If the user is a minor, they need parental consent
                return Ok(new { 
                    message = "Email verification successful. As you are under 18, you need parental consent to fully access the platform.",
                    status = "PendingParentalConsent",
                    requiresParentalConsent = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred during email verification" });
            }
        }
        
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var email = model.Email.ToLower().Trim();
                
                // Check if user exists
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    // For security reasons, don't reveal that the user doesn't exist
                    return Ok(new { message = "If your account exists, a verification email has been sent." });
                }

                // Check if email is already verified
                if (user.IsEmailVerified)
                {
                    // Don't reveal this is the reason, just tell them to try logging in
                    return Ok(new { 
                        message = "Your email is already verified. Please try logging in.", 
                        status = "AlreadyVerified" 
                    });
                }

                // Check if user has exceeded the resend attempts limit
                if (_resendAttempts.TryGetValue(email, out var attempts))
                {
                    if (attempts.Count >= MaxResendAttempts && 
                        DateTime.UtcNow - attempts.LastAttempt < ResendCooldown)
                    {
                        var retryAfterTime = attempts.LastAttempt + ResendCooldown;
                        var minutesLeft = Math.Max(1, (int)(retryAfterTime - DateTime.UtcNow).TotalMinutes);
                        
                        return BadRequest(new { 
                            message = $"Too many attempts. Please try again after {minutesLeft} minutes.", 
                            retryAfter = retryAfterTime,
                            status = "TooManyAttempts"
                        });
                    }
                }

                // Resend verification code via Cognito
                var result = await _cognitoService.ResendConfirmationCodeAsync(email);
                if (!result)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, 
                        new { message = "Failed to resend verification code" });
                }

                // Update resend attempts tracking
                if (_resendAttempts.ContainsKey(email))
                {
                    var currentCount = _resendAttempts[email].Count;
                    _resendAttempts[email] = (currentCount + 1, DateTime.UtcNow);
                }
                else
                {
                    _resendAttempts[email] = (1, DateTime.UtcNow);
                }

                return Ok(new { 
                    message = "Verification email sent. Please check your inbox and spam folder.", 
                    status = "EmailSent"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification email");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while sending the verification email" });
            }
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Authenticate with Cognito
                var authResult = await _cognitoService.LoginAsync(model);
                
                // Get the user from our database
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    // This should not happen if the user was properly created during registration
                    return BadRequest(new { message = "User not found in database. Please contact support." });
                }

                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                
                // Update CognitoUserId if it's empty
                if (string.IsNullOrEmpty(user.CognitoUserId) && !string.IsNullOrEmpty(authResult.UserId))
                {
                    user.CognitoUserId = authResult.UserId;
                }

                // Check if email is verified in Cognito token but not in our database
                bool isEmailVerifiedInToken = false;
                
                // Extract token and check email_verified claim
                if (!string.IsNullOrEmpty(authResult.Token))
                {
                    try
                    {
                        var tokenParts = authResult.Token.Split('.');
                        if (tokenParts.Length >= 2)
                        {
                            // Decode the payload (second part of the token)
                            var payloadBase64 = tokenParts[1];
                            
                            // Ensure proper padding
                            while (payloadBase64.Length % 4 != 0)
                            {
                                payloadBase64 += "=";
                            }
                            
                            // Convert to proper base64url format for decoding
                            payloadBase64 = payloadBase64.Replace('-', '+').Replace('_', '/');
                            
                            // Decode payload
                            var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));
                            
                            // Parse JSON
                            using var jsonDoc = JsonDocument.Parse(payloadJson);
                            
                            // Check if email_verified claim exists and is true
                            isEmailVerifiedInToken = jsonDoc.RootElement.TryGetProperty("email_verified", out var emailVerifiedElement) && 
                                                  emailVerifiedElement.ValueKind == JsonValueKind.True;
                            
                            _logger.LogInformation("Email verified in token: {IsVerified} for user {Email}", isEmailVerifiedInToken, model.Email);
                        }
                    }
                    catch (Exception tokenEx)
                    {
                        _logger.LogError(tokenEx, "Failed to extract verification status from token for user {Email}", model.Email);
                    }
                }
                
                // Update user status if email is verified in token but not in database
                if (isEmailVerifiedInToken && !user.IsEmailVerified)
                {
                    _logger.LogInformation("Automatically updating email verification status for {Email} based on token", model.Email);
                    user.IsEmailVerified = true;
                    user.Status = user.IsMinor ? UserStatus.PendingParentalConsent : UserStatus.Active;
                }
                
                await _dbContext.SaveChangesAsync();

                // Update the response with local database information
                authResult.User.Id = user.Id;
                authResult.User.Status = user.Status.ToString();
                authResult.User.IsEmailVerified = user.IsEmailVerified;
                authResult.User.ParentalConsentGiven = user.ParentalConsentGiven;
                authResult.User.ProfileImageUrl = user.ProfileImageUrl;
                authResult.User.IsProfileComplete = user.IsProfileComplete;

                return Ok(authResult);
            }
            catch (UserNotVerifiedException ex)
            {
                _logger.LogWarning("User {Email} attempted to login but account is not verified", ex.Email);
                
                // Try to automatically resend a verification code for better UX
                try
                {
                    await _cognitoService.ResendConfirmationCodeAsync(model.Email);
                    
                    return BadRequest(new { 
                        message = "Your email address has not been verified. We've sent a new verification code to your email. Please check your inbox and spam folder.",
                        status = "PendingEmailVerification",
                        errorCode = "EMAIL_NOT_VERIFIED",
                        verificationCodeSent = true
                    });
                }
                catch
                {
                    // If resending fails, just return the standard message
                    return BadRequest(new { 
                        message = "Your email address has not been verified. Please check your email for a verification code or request a new one.",
                        status = "PendingEmailVerification",
                        errorCode = "EMAIL_NOT_VERIFIED",
                        verificationCodeSent = false
                    });
                }
            }
            catch (ApplicationException ex) when (ex.Message.Contains("User is not confirmed"))
            {
                _logger.LogWarning(ex, "User {Email} attempted to login but account is not confirmed", model.Email);
                
                // Try to automatically resend a verification code for better UX
                try
                {
                    await _cognitoService.ResendConfirmationCodeAsync(model.Email);
                    
                    return BadRequest(new { 
                        message = "Your email address has not been verified. We've sent a new verification code to your email. Please check your inbox and spam folder.",
                        status = "PendingEmailVerification",
                        errorCode = "EMAIL_NOT_VERIFIED",
                        verificationCodeSent = true
                    });
                }
                catch
                {
                    // If resending fails, just return the standard message
                    return BadRequest(new { 
                        message = "Your email address has not been verified. Please check your email for a verification code or request a new one.",
                        status = "PendingEmailVerification",
                        errorCode = "EMAIL_NOT_VERIFIED",
                        verificationCodeSent = false
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred during login" });
            }
        }
        
        [HttpPost("submit-parent")]
        public async Task<IActionResult> SubmitParent([FromBody] ParentDetailsDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Get the current user
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.StudentEmail);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Verify user is in correct status
                if (user.Status != UserStatus.PendingParentalConsent)
                {
                    return BadRequest(new { 
                        message = "Parental consent is not required or has already been completed",
                        status = user.Status.ToString()
                    });
                }

                // Update parent information
                user.ParentEmail = model.ParentEmail;
                await _dbContext.SaveChangesAsync();

                // Generate a unique token for parent consent
                var consentToken = Guid.NewGuid().ToString();

                // Create or update parental consent record
                var existingConsent = await _dbContext.ParentalConsents.FirstOrDefaultAsync(pc => pc.UserId == user.Id);
                
                if (existingConsent != null)
                {
                    existingConsent.ParentEmail = model.ParentEmail;
                    existingConsent.ConsentToken = consentToken;
                    existingConsent.RequestedAt = DateTime.UtcNow;
                    existingConsent.ExpiresAt = DateTime.UtcNow.AddDays(7);
                    _dbContext.ParentalConsents.Update(existingConsent);
                }
                else
                {
                    var parentalConsent = new ParentalConsent
                    {
                        UserId = user.Id,
                        ParentEmail = model.ParentEmail,
                        ConsentToken = consentToken,
                        IsConsentGiven = false,
                        RequestedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(7)
                    };
                    
                    _dbContext.ParentalConsents.Add(parentalConsent);
                }
                
                await _dbContext.SaveChangesAsync();

                // Send email to parent - wrapped in try/catch to continue even if email fails
                var callbackUrl = $"{_configuration["AppSettings:ApiUrl"]}/api/Auth/parent-consent";
                var emailSent = true;
                
                try
                {
                    await _emailService.SendParentalConsentEmailAsync(
                        model.ParentEmail, 
                        model.ParentName, 
                        user.FullName, 
                        consentToken, 
                        callbackUrl);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send parental consent email, but request was processed");
                    emailSent = false;
                }

                // For testing purposes, include the consent token in the response
                var response = new { 
                    message = emailSent 
                        ? "Parental consent request sent successfully" 
                        : "Parental consent request processed but email failed to send",
                    status = "PendingParentalConsent",
                    consentUrl = $"{callbackUrl}?token={consentToken}"
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending parental consent request");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while processing your request" });
            }
        }
        
        [HttpGet("parent-consent")]
        public async Task<IActionResult> ParentConsent([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            try
            {
                // Find the parental consent record with this token
                var parentalConsent = await _dbContext.ParentalConsents
                    .Include(pc => pc.User)
                    .FirstOrDefaultAsync(pc => pc.ConsentToken == token && !pc.IsConsentGiven);
                
                if (parentalConsent == null)
                {
                    return NotFound(new { message = "Invalid or expired consent request" });
                }

                if (DateTime.UtcNow > parentalConsent.ExpiresAt)
                {
                    return BadRequest(new { message = "Consent request has expired. Please request a new one." });
                }

                // Update the parental consent
                parentalConsent.IsConsentGiven = true;
                parentalConsent.ConsentGivenAt = DateTime.UtcNow;
                
                // Update the user status
                var user = parentalConsent.User;
                user.ParentalConsentGiven = true;
                user.ParentalConsentDate = DateTime.UtcNow;
                user.Status = UserStatus.Active;
                
                await _dbContext.SaveChangesAsync();

                // Redirect to success page
                return Redirect($"{_configuration["AppSettings:ApiUrl"]}/api/Auth/email-verified?status=ParentalConsentSuccess");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming parental consent");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while processing your request" });
            }
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailRedirect([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            try
            {
                // Find the verification record
                var verification = await _dbContext.EmailVerifications
                    .FirstOrDefaultAsync(v => v.VerificationCode == token && !v.IsUsed && v.Type == VerificationType.EmailVerification);

                if (verification == null || DateTime.UtcNow > verification.ExpiresAt)
                {
                    _logger.LogWarning("Invalid or expired verification token: {Token}", token);
                    return Redirect($"{_configuration["AppSettings:WebsiteUrl"]}/auth/verification-error");
                }

                var email = verification.Email;

                // Find the user in our database
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    _logger.LogWarning("User not found in database during email verification: {Email}", email);
                    return Redirect($"{_configuration["AppSettings:WebsiteUrl"]}/auth/verification-error");
                }

                // Mark verification as used
                verification.IsUsed = true;
                verification.UsedAt = DateTime.UtcNow;

                // Update user status
                if (!user.IsEmailVerified)
                {
                    user.IsEmailVerified = true;
                    user.Status = user.IsMinor ? UserStatus.PendingParentalConsent : UserStatus.Active;
                    
                    // Confirm user in Cognito
                    try
                    {
                        await _cognitoService.AdminConfirmSignUpAsync(email);
                    }
                    catch (Exception cognitoEx)
                    {
                        _logger.LogError(cognitoEx, "Error confirming user in Cognito for {Email}", email);
                        // We continue because the local DB is updated
                    }

                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Updated user email verification status for {Email}", email);
                }

                // Redirect to success page
                return Redirect($"{_configuration["AppSettings:ApiUrl"]}/api/Auth/email-verified?status={user.Status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email verification for token {Token}", token);
                return Redirect($"{_configuration["AppSettings:ApiUrl"]}/api/Auth/email-verified?status=Error");
            }
        }

        [HttpGet("email-verified")]
        public IActionResult EmailVerified([FromQuery] string status)
        {
            var title = "Email Verified!";
            var message = "Your email has been successfully verified.";
            var subMessage = "You can now return to the GateSale app.";
            var color = "#22C55E"; // Success green

            if (status == "PendingParentalConsent")
            {
                title = "Email Verified!";
                message = "Your email is verified, but we need parental consent.";
                subMessage = "Please check your parent's email for the consent link.";
                color = "#3B82F6"; // Info blue
            }
            else if (status == "ParentalConsentSuccess")
            {
                title = "Consent Confirmed!";
                message = "Thank you! You have successfully given parental consent.";
                subMessage = "Your child can now fully access the GateSale platform.";
                color = "#22C55E"; // Success green
            }
            else if (status == "Error")
            {
                title = "Verification Error";
                message = "We couldn't verify your email.";
                subMessage = "The link may be expired or invalid. Please try resending the verification email from the app.";
                color = "#EF4444"; // Error red
            }

            var html = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; display: flex; align-items: center; justify-content: center; height: 100vh; margin: 0; background-color: #F9FAFB; }}
        .card {{ background: white; padding: 2rem; border-radius: 1rem; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1); text-align: center; max-width: 400px; width: 90%; }}
        .icon {{ font-size: 4rem; color: {color}; margin-bottom: 1rem; }}
        h1 {{ color: #111827; margin-bottom: 0.5rem; font-size: 1.5rem; }}
        p {{ color: #4B5563; line-height: 1.5; margin-bottom: 1.5rem; }}
        .btn {{ display: inline-block; background-color: #3B82F6; color: white; padding: 0.75rem 1.5rem; border-radius: 0.5rem; text-decoration: none; font-weight: 600; transition: background-color 0.2s; }}
        .btn:hover {{ background-color: #2563EB; }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""icon"">{(status == "Error" ? "✕" : "✓")}</div>
        <h1>{title}</h1>
        <p>{message}<br><br>{subMessage}</p>
        <a href=""#"" onclick=""window.close(); return false;"" class=""btn"">Close Window</a>
    </div>
</body>
</html>";

            return Content(html, "text/html");
        }
        
        [HttpPost("sync-verification-status")]
        public async Task<IActionResult> SyncVerificationStatus([FromBody] string email, [FromHeader(Name = "Authorization")] string authorization)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email is required" });
            }
            
            try
            {
                // Get the JWT token from the Authorization header
                if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                {
                    return BadRequest(new { message = "Authorization header with Bearer token is required" });
                }
                
                var token = authorization.Substring("Bearer ".Length).Trim();
                bool isEmailVerified = false;
                
                try
                {
                    // Decode the JWT token (just basic string manipulation, not validation)
                    var tokenParts = token.Split('.');
                    if (tokenParts.Length >= 2)
                    {
                        // Decode the payload (second part of the token)
                        var payloadBase64 = tokenParts[1];
                        
                        // Ensure proper padding
                        while (payloadBase64.Length % 4 != 0)
                        {
                            payloadBase64 += "=";
                        }
                        
                        // Convert to proper base64url format for decoding
                        payloadBase64 = payloadBase64.Replace('-', '+').Replace('_', '/');
                        
                        // Decode payload
                        var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));
                        
                        // Parse JSON
                        using var jsonDoc = JsonDocument.Parse(payloadJson);
                        
                        // Check if email_verified claim exists and is true
                        isEmailVerified = jsonDoc.RootElement.TryGetProperty("email_verified", out var emailVerifiedElement) && 
                                        emailVerifiedElement.ValueKind == JsonValueKind.True;
                    }
                }
                catch (Exception tokenEx)
                {
                    _logger.LogError(tokenEx, "Failed to extract verification status from token");
                    return BadRequest(new { message = "Invalid token format" });
                }
                
                if (isEmailVerified)
                {
                    // Update local database
                    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (user != null && !user.IsEmailVerified)
                    {
                        user.IsEmailVerified = true;
                        user.Status = user.IsMinor ? UserStatus.PendingParentalConsent : UserStatus.Active;
                        await _dbContext.SaveChangesAsync();
                        
                        _logger.LogInformation("Manually synced verification status from JWT token for {Email}", email);
                        
                        return Ok(new { 
                            message = "Verification status synced successfully", 
                            status = user.Status.ToString(),
                            isEmailVerified = user.IsEmailVerified,
                            userId = user.Id
                        });
                    }
                    else if (user != null && user.IsEmailVerified)
                    {
                        return Ok(new { 
                            message = "Email is already verified", 
                            status = user.Status.ToString(),
                            isEmailVerified = user.IsEmailVerified,
                            userId = user.Id
                        });
                    }
                }
                
                return BadRequest(new { message = "Could not sync verification status. Email may not be verified." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing verification status for {Email}", email);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while syncing verification status" });
            }
        }

        /// <summary>
        /// Check if user's email has been verified (via email button click).
        /// Used by mobile app to poll verification status.
        /// </summary>
        [HttpGet("check-verification-status")]
        public async Task<IActionResult> CheckVerificationStatus([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            try
            {
                email = email.ToLower().Trim();
                
                // Check local database first
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return NotFound(new { 
                        message = "User not found", 
                        isVerified = false 
                    });
                }

                // If already verified in our DB, return true
                if (user.IsEmailVerified)
                {
                    return Ok(new { 
                        isVerified = true, 
                        status = user.Status.ToString(),
                        message = "Email is verified"
                    });
                }

                // Not verified yet
                return Ok(new { 
                    isVerified = false, 
                    status = "PendingEmailVerification",
                    message = "Email not yet verified"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking verification status for {Email}", email);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while checking verification status" });
            }
        }
        
        private bool IsMinor(DateTime dateOfBirth)
        {
            var today = DateTime.UtcNow.Date;
            var age = today.Year - dateOfBirth.Year;
            
            // Adjust age if the birthday hasn't occurred yet this year
            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }
            
            return age < 30;
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var email = model.Email.ToLower().Trim();
                
                // Check if user exists in our database
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    // For security reasons, don't reveal that the user doesn't exist
                    return Ok(new { message = "If your account exists, a password reset code has been sent to your email." });
                }

                // Generate a 6-digit numeric code
                var random = new Random();
                var resetCode = random.Next(100000, 999999).ToString();

                // Store in EmailVerifications
                var emailVerification = new EmailVerification
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    VerificationCode = resetCode,
                    Type = VerificationType.PasswordReset,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(1) // Reset codes expire in 1 hour
                };
                _dbContext.EmailVerifications.Add(emailVerification);
                await _dbContext.SaveChangesAsync();

                // Send email in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendPasswordResetEmailAsync(email, resetCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending password reset email to {Email}", email);
                    }
                });
                
                return Ok(new { 
                    message = "A password reset code has been sent to your email. The code is 6 digits.",
                    email = email,
                    status = "CodeSent" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting password reset");
                return Ok(new { message = "If your account exists, a password reset code has been sent to your email." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var email = model.Email.ToLower().Trim();
                var code = model.Code.Trim();

                _logger.LogInformation("ResetPassword attempt for Email: {Email}, Code: {Code}", email, code);

                // Verify code again before resetting
                // Use ToLower() on the database field for robust comparison
                var verification = await _dbContext.EmailVerifications
                    .Where(v => v.Email.ToLower() == email && 
                               v.VerificationCode == code && 
                               v.Type == VerificationType.PasswordReset && 
                               !v.IsUsed)
                    .OrderByDescending(v => v.CreatedAt)
                    .FirstOrDefaultAsync();

                if (verification == null)
                {
                    _logger.LogWarning("No reset record found for {Email} with code {Code}", email, code);
                    return BadRequest(new { message = "Invalid verification code. Please request a new one." });
                }

                if (verification.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Reset code expired for {Email}. ExpiresAt: {ExpiresAt}, CurrentTime: {CurrentTime}", 
                        email, verification.ExpiresAt, DateTime.UtcNow);
                    return BadRequest(new { message = "Verification code has expired. Please request a new one." });
                }

                // Update password in Cognito
                var result = await _cognitoService.AdminSetUserPasswordAsync(email, model.NewPassword);
                if (!result)
                {
                    return BadRequest(new { message = "Password reset failed. Please try again." });
                }

                // Mark code as used
                verification.IsUsed = true;
                verification.UsedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                
                return Ok(new { 
                    message = "Your password has been reset successfully. You can now log in with your new password.",
                    status = "PasswordReset" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while resetting your password." });
            }
        }

        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var email = model.Email.ToLower().Trim();
                var code = model.Code.Trim();

                _logger.LogInformation("VerifyResetCode attempt for Email: {Email}, Code: {Code}", email, code);

                var verification = await _dbContext.EmailVerifications
                    .Where(v => v.Email.ToLower() == email && 
                               v.VerificationCode == code && 
                               v.Type == VerificationType.PasswordReset && 
                               !v.IsUsed)
                    .OrderByDescending(v => v.CreatedAt)
                    .FirstOrDefaultAsync();

                if (verification == null)
                {
                    _logger.LogWarning("VerifyResetCode failed for {Email}: No matching unused record found.", email);
                    return Ok(new { 
                        message = "Invalid verification code. Please check and try again.",
                        isValid = false 
                    });
                }

                if (verification.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("VerifyResetCode failed for {Email}: Code expired at {ExpiresAt}", email, verification.ExpiresAt);
                    return Ok(new { 
                        message = "Verification code has expired. Please request a new one.",
                        isValid = false 
                    });
                }

                return Ok(new { 
                    message = "Code verified successfully. You can now reset your password.",
                    isValid = true 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reset code");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while verifying your code." });
            }
        }

        [HttpPost("admin-manual-reset")]
        public async Task<IActionResult> AdminManualReset([FromBody] ResetPasswordDto model)
        {
            // This is a temporary endpoint for manual resets as requested by the user
            // In a real app, this would be protected by admin roles
            try
            {
                var email = model.Email.ToLower().Trim();
                _logger.LogInformation("ADMIN: Manual password reset for {Email}", email);

                var result = await _cognitoService.AdminSetUserPasswordAsync(email, model.NewPassword);
                if (!result)
                {
                    return BadRequest(new { message = "Manual reset failed. User might not exist in Cognito." });
                }

                return Ok(new { message = $"Password for {email} has been manually reset successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in manual reset");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    
    
        [HttpGet("check-school")]
        public async Task<IActionResult> CheckSchool([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "School name is required." });
            }

            try
            {
                var schoolName = name.Trim().ToLower();
                
                // 1. Check if school is already whitelisted
                var school = await _dbContext.WhitelistedDomains
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.SchoolName.ToLower() == schoolName && w.IsActive);

                if (school != null)
                {
                    return Ok(new
                    {
                        Id = school.Id.ToString(),
                        Name = school.SchoolName,
                        IsVerified = school.IsActive,
                        IsActive = true
                    });
                }

                // 2. Check if there's a pending request for this school
                var pendingRequest = await _dbContext.SchoolRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.SchoolName.ToLower() == schoolName);

                if (pendingRequest != null)
                {
                    return Ok(new
                    {
                        Id = "pending-" + pendingRequest.Id.ToString(),
                        Name = pendingRequest.SchoolName,
                        IsVerified = false,
                        IsActive = true,
                        IsPending = true
                    });
                }

                return NotFound(new { message = "School not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking school: {Name}", name);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while checking the school." });
            }
        }

        [HttpPost("request-school")]
        public async Task<IActionResult> RequestSchool([FromBody] SchoolRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var schoolName = model.SchoolName.Trim();
                var city = model.City.Trim();

                // Check if school is already whitelisted
                var isWhitelisted = await _dbContext.WhitelistedDomains
                    .AnyAsync(w => w.SchoolName.ToLower() == schoolName.ToLower());
                
                if (isWhitelisted)
                {
                    _logger.LogInformation("School {SchoolName} is already whitelisted. Proceeding.", schoolName);
                    return Ok(new { message = "This school is already supported. Proceeding with registration." });
                }

                // Check if there's already a pending request for this school
                var existingRequest = await _dbContext.SchoolRequests
                    .AnyAsync(r => r.SchoolName.ToLower() == schoolName.ToLower() && r.City.ToLower() == city.ToLower());

                if (existingRequest)
                {
                    _logger.LogInformation("School request for {SchoolName} already exists. Proceeding.", schoolName);
                    return Ok(new { message = "A request for this school is already being reviewed. Proceeding with registration." });
                }

                // Create new school request
                var schoolRequest = new SchoolRequest
                {
                    Id = Guid.NewGuid(),
                    SchoolName = schoolName,
                    City = city,
                    RequesterEmail = model.RequesterEmail,
                    ContactPerson = model.ContactPerson,
                    Status = SchoolRequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Saving school request for {SchoolName} in {City}", schoolName, city);
                _dbContext.SchoolRequests.Add(schoolRequest);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("School request saved successfully for {SchoolName}", schoolName);
                return Ok(new { message = "School request submitted successfully. We will review it and notify you." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting school request");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while submitting your request." });
            }
        }
    }

    public class SchoolRequestDto
    {
        [Required]
        public required string SchoolName { get; set; }
        [Required]
        public required string City { get; set; }
        [Required]
        [EmailAddress]
        public required string RequesterEmail { get; set; }
        public string? ContactPerson { get; set; }
    }

    public class ParentDetailsDto
    {
        [Required]
        public required string ParentName { get; set; }
        
        [Required]
        [EmailAddress]
        public required string ParentEmail { get; set; }
        
        [Required]
        [EmailAddress]
        public required string StudentEmail { get; set; }
    }

    public class ResendVerificationDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
} 