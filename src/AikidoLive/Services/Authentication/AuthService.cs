using AikidoLive.DataModels;
using AikidoLive.Services.DBConnector;
using AikidoLive.Services.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace AikidoLive.Services.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly DBServiceConnector _dbServiceConnector;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AuthService(DBServiceConnector dbServiceConnector, IEmailService emailService, 
            IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _dbServiceConnector = dbServiceConnector;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            var users = await _dbServiceConnector.GetUsers();
            if (users == null || !users.Any())
                return null;

            var userList = users.FirstOrDefault();
            if (userList?.Users == null)
                return null;

            var user = userList.Users.FirstOrDefault(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            
            if (user == null)
                return null;

            // Check if email is confirmed
            if (!user.IsEmailConfirmed)
                return null;

            if (!VerifyPassword(password, user.Password))
                return null;

            return user;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try 
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        public async Task<User> RegisterUserAsync(RegisterModel model)
        {
            var users = await _dbServiceConnector.GetUsers();
            if (users == null || !users.Any())
                throw new Exception("User database not found");

            var userList = users.FirstOrDefault();
            if (userList?.Users == null)
                throw new Exception("User list not initialized");

            // Check if email already exists
            if (userList.Users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
                return null;

            // Generate email confirmation token
            var confirmationToken = GenerateSecureToken();
            
            // Create new user
            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Role = "User",
                Password = HashPassword(model.Password),
                IsEmailConfirmed = false,
                EmailConfirmationToken = confirmationToken,
                EmailConfirmationTokenExpiry = DateTime.UtcNow.AddDays(7) // Token expires in 7 days
            };

            // Add user to the database
            userList.Users.Add(newUser);
            await _dbServiceConnector.UpdateUser(userList);
            
            // Send confirmation email
            var confirmationLink = BuildConfirmationLink(confirmationToken);
            await _emailService.SendConfirmationEmailAsync(newUser.Email, newUser.FirstName, confirmationLink);
            
            return newUser;
        }

        public async Task<bool> ConfirmEmailAsync(string token)
        {
            var users = await _dbServiceConnector.GetUsers();
            if (users == null || !users.Any())
                return false;

            var userList = users.FirstOrDefault();
            if (userList?.Users == null)
                return false;

            var user = userList.Users.FirstOrDefault(u => 
                u.EmailConfirmationToken == token && 
                u.EmailConfirmationTokenExpiry > DateTime.UtcNow);

            if (user == null)
                return false;

            // Confirm the email
            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiry = null;

            await _dbServiceConnector.UpdateUser(userList);
            return true;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null || !user.IsEmailConfirmed)
                return false;

            var users = await _dbServiceConnector.GetUsers();
            var userList = users.FirstOrDefault();
            
            // Generate password reset token
            var resetToken = GenerateSecureToken();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

            await _dbServiceConnector.UpdateUser(userList);

            // Send password reset email
            var resetLink = BuildPasswordResetLink(resetToken);
            return await _emailService.SendPasswordResetEmailAsync(user.Email, user.FirstName, resetLink);
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var users = await _dbServiceConnector.GetUsers();
            if (users == null || !users.Any())
                return false;

            var userList = users.FirstOrDefault();
            if (userList?.Users == null)
                return false;

            var user = userList.Users.FirstOrDefault(u => 
                u.PasswordResetToken == token && 
                u.PasswordResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
                return false;

            // Reset the password
            user.Password = HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _dbServiceConnector.UpdateUser(userList);
            return true;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var users = await _dbServiceConnector.GetUsers();
            if (users == null || !users.Any())
                return null;

            var userList = users.FirstOrDefault();
            if (userList?.Users == null)
                return null;

            return userList.Users.FirstOrDefault(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        private string GenerateSecureToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            }
        }

        private string BuildConfirmationLink(string token)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}";
            return $"{baseUrl}/Account/ConfirmEmail?token={Uri.EscapeDataString(token)}";
        }

        private string BuildPasswordResetLink(string token)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}";
            return $"{baseUrl}/Account/ResetPassword?token={Uri.EscapeDataString(token)}";
        }
    }
}