using AikidoLive.DataModels;
using AikidoLive.Services.DBConnector;
using AikidoLive.Services.Email;
using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace AikidoLive.Services.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly DBServiceConnector _dbServiceConnector;
        private readonly IEmailService _emailService;

        public AuthService(DBServiceConnector dbServiceConnector, IEmailService emailService)
        {
            _dbServiceConnector = dbServiceConnector;
            _emailService = emailService;
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

            // Create new user
            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Role = "User",
                Password = HashPassword(model.Password)
            };

            // Add user to the database
            userList.Users.Add(newUser);
            await _dbServiceConnector.UpdateUser(userList);
            
            // Send notification to admins (non-blocking)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendNewUserNotificationToAdminsAsync(
                        newUser.FirstName, 
                        newUser.LastName, 
                        newUser.Email);
                }
                catch (Exception)
                {
                    // Email failure should not affect user registration
                    // Logging is handled within the email service
                }
            });
            
            return newUser;
        }
    }
}