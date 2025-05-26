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

        public User()
        {
            FirstName = "";
            LastName = "";
            Email = "";
            Role = "";
            Password = "";
        }
        public User(string firstName, string lastName, string email, string role, string password)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Role = role;
            Password = password;
        }
    }
}