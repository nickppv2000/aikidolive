using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AikidoLive.DataModels
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }

        public User()
        {
            FirstName = "";
            LastName = "";
        }

        public User(IdentityUser identityUser)
        {
            // Copy properties from identityUser to this
            this.Id = identityUser.Id;
            this.UserName = identityUser.UserName;
            this.NormalizedUserName = identityUser.NormalizedUserName;
            this.Email = identityUser.Email;
            this.NormalizedEmail = identityUser.NormalizedEmail;
            this.EmailConfirmed = identityUser.EmailConfirmed;
            this.PasswordHash = identityUser.PasswordHash;
            this.SecurityStamp = identityUser.SecurityStamp;
            this.ConcurrencyStamp = identityUser.ConcurrencyStamp;
            this.PhoneNumber = identityUser.PhoneNumber;
            this.PhoneNumberConfirmed = identityUser.PhoneNumberConfirmed;
            this.TwoFactorEnabled = identityUser.TwoFactorEnabled;
            this.LockoutEnd = identityUser.LockoutEnd;
            this.LockoutEnabled = identityUser.LockoutEnabled;
            this.AccessFailedCount = identityUser.AccessFailedCount;

            // Initialize other properties
            this.FirstName = "";
            this.LastName = "";
            this.Role = "";
        }

        public User(string firstName, string lastName, string email, string role, string password)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            UserName = email;
            Role = role;
            PasswordHash = password;
        }
    }
}