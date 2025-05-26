using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AikidoLive.DataModels
{
    public class User
    {
        public string Id { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        
        public string Role { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        
        // Salt for password hashing
        public string PasswordSalt { get; set; }

        public User()
        {
            Id = Guid.NewGuid().ToString();
            FirstName = "";
            LastName = "";
            Email = "";
            Role = "User";
            Password = "";
            PasswordSalt = "";
        }
        
        public User(string firstName, string lastName, string email, string role, string password)
        {
            Id = Guid.NewGuid().ToString();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Role = role ?? "User";
            Password = password;
            PasswordSalt = "";
        }
    }
}