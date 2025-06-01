using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AikidoLive.DataModels
{
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role {get; set; }
        public string Password { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public User()
        {
            FirstName = "";
            LastName = "";
            Email = "";
            Role = "";
            Password = "";
            IsEmailConfirmed = false;
        }

        public User(string firstName, string lastName, string email, string role, string password)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Role = role;
            Password = password;
            IsEmailConfirmed = false;
        }
    }
}