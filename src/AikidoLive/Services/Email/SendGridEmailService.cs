using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace AikidoLive.Services.Email
{
    public class SendGridEmailService : IEmailService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
        {
            var apiKey = configuration["SendGrid:ApiKey"];
            _fromEmail = configuration["SendGrid:FromEmail"];
            _fromName = configuration["SendGrid:FromName"];
            _sendGridClient = new SendGridClient(apiKey);
            _logger = logger;
        }

        public async Task<bool> SendConfirmationEmailAsync(string toEmail, string firstName, string confirmationLink)
        {
            try
            {
                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail, firstName);
                var subject = "Confirm Your Aikido Live Account";
                
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #2c3e50;'>Welcome to Aikido Live, {firstName}!</h2>
                        <p>Thank you for registering with Aikido Live. To complete your registration and activate your account, please click the link below:</p>
                        <div style='text-align: center; margin: 20px 0;'>
                            <a href='{confirmationLink}' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>Confirm Your Account</a>
                        </div>
                        <p>If you didn't create an account with Aikido Live, please ignore this email.</p>
                        <p>Best regards,<br>The Aikido Live Team</p>
                    </div>";

                var plainTextContent = $@"
                    Welcome to Aikido Live, {firstName}!
                    
                    Thank you for registering with Aikido Live. To complete your registration and activate your account, please visit:
                    {confirmationLink}
                    
                    If you didn't create an account with Aikido Live, please ignore this email.
                    
                    Best regards,
                    The Aikido Live Team";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await _sendGridClient.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Confirmation email sent successfully to {toEmail}");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to send confirmation email to {toEmail}. Status: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending confirmation email to {toEmail}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string firstName, string resetLink)
        {
            try
            {
                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail, firstName);
                var subject = "Reset Your Aikido Live Password";
                
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #2c3e50;'>Password Reset Request</h2>
                        <p>Hello {firstName},</p>
                        <p>We received a request to reset your password for your Aikido Live account. Click the link below to reset your password:</p>
                        <div style='text-align: center; margin: 20px 0;'>
                            <a href='{resetLink}' style='background-color: #e74c3c; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>Reset Your Password</a>
                        </div>
                        <p>This link will expire in 24 hours for security purposes.</p>
                        <p>If you didn't request a password reset, please ignore this email. Your password will remain unchanged.</p>
                        <p>Best regards,<br>The Aikido Live Team</p>
                    </div>";

                var plainTextContent = $@"
                    Password Reset Request
                    
                    Hello {firstName},
                    
                    We received a request to reset your password for your Aikido Live account. Visit the link below to reset your password:
                    {resetLink}
                    
                    This link will expire in 24 hours for security purposes.
                    
                    If you didn't request a password reset, please ignore this email. Your password will remain unchanged.
                    
                    Best regards,
                    The Aikido Live Team";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await _sendGridClient.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Password reset email sent successfully to {toEmail}");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to send password reset email to {toEmail}. Status: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending password reset email to {toEmail}");
                return false;
            }
        }
    }
}