using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AikidoLive.Services.DBConnector;
using AikidoLive.DataModels;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AikidoLive.Services
{
    public class UserService
    {
        private readonly DBServiceConnector _dbServiceConnector;
        private readonly ILogger<UserService> _logger;
        public List<UserList> _libUserList;

        public bool IsAuthenticated { get; set; }

        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UserService( ILogger<UserService> logger, 
                            DBServiceConnector dbServiceConnector, 
                            SignInManager<IdentityUser> signInManager,
                            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _dbServiceConnector = dbServiceConnector;
            _signInManager = signInManager;
            _userManager   = userManager;
            _libUserList = new List<UserList>();
        }

        public async Task<User> GetUserByEmail(string email)
        {
            _libUserList = await _dbServiceConnector.GetUsers();
            foreach (var userList in _libUserList)
            {
                foreach (var user in userList.Users)
                {
                    if (user.Email == email)
                    {
                        return user;
                    }
                }
            }
            // Implement logic to retrieve user from Cosmos DB by email
            return new User();
        }

        public async Task<IdentityResult> RegisterUser(string username, string password)
        {
            var user = new IdentityUser { UserName = username, Email = username};
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                IsAuthenticated = true;
            }
            else
            {
                IsAuthenticated = false;
            }

            return result;
        }
        
        public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<SignInResult> LoginUser(string username, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                IsAuthenticated = true;
            }
            else
            {
                IsAuthenticated = false;
            }

            return result;
        }

        public async Task LogoutUser()
        {
            await _signInManager.SignOutAsync();
            IsAuthenticated = false;
        }

        

        public async Task<IdentityResult> AddUserToRole(string username, string role)
        {
            var user = await _userManager.FindByNameAsync(username);
            var result = await _userManager.AddToRoleAsync(user, role);
            return result;
        }

        public async Task<IdentityResult> ChangePassword(string username, string oldPassword, string newPassword)
        {
            var user = await _userManager.FindByNameAsync(username);
            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            return result;
        }

        public async Task<IdentityResult> UpdateUser(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, password);
            var result = await _userManager.UpdateAsync(user);
            return result;
        }

        /*public async Task<IdentityResult> DeleteUser(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            var result = await _userManager.DeleteAsync(user);
            return result;
        }*/



    }
}