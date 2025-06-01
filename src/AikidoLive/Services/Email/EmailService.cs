using AikidoLive.DataModels;
using AikidoLive.Services.DBConnector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AikidoLive.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly DBServiceConnector _dbServiceConnector;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(DBServiceConnector dbServiceConnector, IConfiguration configuration, ILogger<EmailService> logger)
        {
            _dbServiceConnector = dbServiceConnector;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendNewUserNotificationToAdminsAsync(string newUserFirstName, string newUserLastName, string newUserEmail)
        {
            try
            {
                // Get all admin users
                var adminEmails = await GetAdminEmailsAsync();
                if (!adminEmails.Any())
                {
                    _logger.LogWarning("No admin users found to notify for new user registration");
                    return true; // Not a failure, just no admins to notify
                }

                // Get email configuration
                var emailConfig = _configuration.GetSection("Email");
                var smtpHost = emailConfig["SmtpHost"];
                var smtpPort = int.Parse(emailConfig["SmtpPort"] ?? "587");
                var smtpUsername = emailConfig["SmtpUsername"];
                var smtpPassword = emailConfig["SmtpPassword"];
                var fromEmail = emailConfig["FromEmail"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogWarning("Email configuration is incomplete. Skipping admin notification.");
                    return true; // Don't fail registration if email is not configured
                }

                // Prepare email content
                var subject = "New User Registration - Aikido Live";
                var body = $@"
                    <h2>New User Registration</h2>
                    <p>A new user has registered on the Aikido Live platform:</p>
                    <ul>
                        <li><strong>Name:</strong> {newUserFirstName} {newUserLastName}</li>
                        <li><strong>Email:</strong> {newUserEmail}</li>
                        <li><strong>Registration Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</li>
                    </ul>
                    <p>This is an automated notification from the Aikido Live system.</p>
                ";

                // Send email to each admin
                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                foreach (var adminEmail in adminEmails)
                {
                    try
                    {
                        var mailMessage = new MailMessage(fromEmail, adminEmail, subject, body)
                        {
                            IsBodyHtml = true
                        };

                        await smtpClient.SendMailAsync(mailMessage);
                        _logger.LogInformation($"New user notification sent to admin: {adminEmail}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send new user notification to admin: {adminEmail}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending new user notifications to admins");
                return false; // Return false but don't throw - email failure shouldn't break registration
            }
        }

        private async Task<List<string>> GetAdminEmailsAsync()
        {
            try
            {
                var userListDocuments = await _dbServiceConnector.GetUsers();
                if (userListDocuments?.Any() != true)
                    return new List<string>();

                var userList = userListDocuments.FirstOrDefault();
                if (userList?.Users == null)
                    return new List<string>();

                return userList.Users
                    .Where(u => u.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
                    .Select(u => u.Email)
                    .Where(email => !string.IsNullOrEmpty(email))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin users for email notification");
                return new List<string>();
            }
        }
    }
}