using GateSale.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace GateSale.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _fromEmail;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _enableActualEmailSending;
        private readonly string _environment;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            _fromEmail = _configuration["GoogleSMTP:Username"];
            _smtpServer = _configuration["GoogleSMTP:SmtpServer"];
            _smtpPort = int.Parse(_configuration["GoogleSMTP:Port"]);
            _smtpUsername = _configuration["GoogleSMTP:Username"];
            _smtpPassword = _configuration["GoogleSMTP:Password"];
            
            _environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            _enableActualEmailSending = bool.TryParse(_configuration["EmailSettings:EnableActualSending"], out var enabled) ? enabled : false;
            
            _logger.LogInformation($"Email service initialized in {_environment} mode. Actual sending: {_enableActualEmailSending}");
        }

        public async Task SendVerificationEmailAsync(string email, string token, string callbackUrl)
        {
            var subject = "Verify your GateSale account";
            var body = $@"
                <h2>Welcome to GateSale!</h2>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href='{callbackUrl}?token={WebUtility.UrlEncode(token)}'>Verify Email Address</a></p>
                <p>If you did not create this account, please ignore this email.</p>
                <p>Thank you,<br>The GateSale Team</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendParentalConsentEmailAsync(string parentEmail, string parentName, string studentName, string consentToken, string callbackUrl)
        {
            var subject = $"Parental Consent Request for {studentName}'s GateSale Account";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e5e7eb; border-radius: 8px;'>
                    <h2 style='color: #111827;'>Hi {parentName},</h2>
                    <p style='color: #4b5563; font-size: 16px; line-height: 1.5;'>Your child, <strong>{studentName}</strong>, has signed up to use GateSale — a safe online marketplace exclusively for school learners.</p>
                    <p style='color: #4b5563; font-size: 16px; line-height: 1.5;'>To protect all users, we require parental or guardian consent before allowing access to post or interact on the platform.</p>
                    
                    <h3 style='color: #111827;'>What is GateSale?</h3>
                    <p style='color: #4b5563; font-size: 14px;'>GateSale lets students buy and sell items like textbooks, uniforms, and electronics within their school community, in a secure, moderated environment.</p>
                    
                    <h3 style='color: #111827;'>What You're Approving</h3>
                    <p style='color: #4b5563; font-size: 14px;'>By clicking the button below, you're giving permission for your child to use GateSale under our community guidelines.</p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{callbackUrl}?token={WebUtility.UrlEncode(consentToken)}' 
                           style='background-color: #3B82F6; color: white; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-weight: bold; display: inline-block;'>
                           Approve Access Now
                        </a>
                    </div>
                    
                    <p style='color: #6b7280; font-size: 12px;'>If you do not recognize this request, simply ignore this email and no account will be activated.</p>
                    <hr style='border: 0; border-top: 1px solid #e5e7eb; margin: 20px 0;'>
                    <p style='color: #9ca3af; font-size: 12px;'>Warm regards,<br>The GateSale Team</p>
                </div>
            ";

            await SendEmailAsync(parentEmail, subject, body);
            _logger.LogInformation("Parental consent URL: {0}?token={1}", callbackUrl, consentToken);
        }

        public async Task SendPasswordResetEmailAsync(string email, string code)
        {
            var subject = "Reset your GateSale password";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e5e7eb; border-radius: 8px;'>
                    <h2 style='color: #111827;'>Password Reset Request</h2>
                    <p style='color: #4b5563; font-size: 16px; line-height: 1.5;'>We received a request to reset your password. Use the following 6-digit code in the app to set a new password:</p>
                    <div style='background-color: #f3f4f6; padding: 20px; text-align: center; border-radius: 8px; margin: 20px 0;'>
                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #1f2937;'>{code}</span>
                    </div>
                    <p style='color: #4b5563; font-size: 14px;'>This code will expire in 24 hours.</p>
                    <p style='color: #6b7280; font-size: 14px; margin-top: 20px;'>If you did not request a password reset, please ignore this email.</p>
                    <hr style='border: 0; border-top: 1px solid #e5e7eb; margin: 20px 0;'>
                    <p style='color: #9ca3af; font-size: 12px;'>Thank you,<br>The GateSale Team</p>
                </div>
            ";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            if (_environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Email to: {0}, Subject: {1}, Body: {2}", to, subject, htmlBody);
            }
            
            if (!_enableActualEmailSending && _environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Email sending suppressed in development mode. Would have sent email to {0}", to);
                return;
            }
            
            try
            {
                using var client = new SmtpClient(_smtpServer, _smtpPort);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                client.EnableSsl = true;
                client.Timeout = 10000;

                using var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, "GateSale");
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true;
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                if (!_environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
                {
                    throw;
                }
            }
        }
    }
}