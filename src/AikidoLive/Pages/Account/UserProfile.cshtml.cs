using AikidoLive.DataModels;
using AikidoLive.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace AikidoLive.Pages.Account
{
    [Authorize]
    public class UserProfilePageModel : PageModel
    {
        private readonly IAuthService _authService;

        [BindProperty]
        public UserProfileModel Profile { get; set; } = new UserProfileModel();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public UserProfilePageModel(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Profile.FirstName = user.FirstName;
            Profile.LastName = user.LastName;
            Profile.Email = user.Email;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            var currentUser = await _authService.GetUserByEmailAsync(userEmail);
            if (currentUser == null)
            {
                ErrorMessage = "User not found.";
                return Page();
            }

            // Validate current password if user wants to change password
            if (!string.IsNullOrEmpty(Profile.NewPassword))
            {
                if (string.IsNullOrEmpty(Profile.CurrentPassword))
                {
                    ErrorMessage = "Current password is required to change password.";
                    return Page();
                }

                if (!_authService.VerifyPassword(Profile.CurrentPassword, currentUser.Password))
                {
                    ErrorMessage = "Current password is incorrect.";
                    return Page();
                }
            }

            // Check if email is being changed and if it's already in use
            if (!Profile.Email.Equals(currentUser.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingUserWithEmail = await _authService.GetUserByEmailAsync(Profile.Email);
                if (existingUserWithEmail != null)
                {
                    ErrorMessage = "Email is already in use by another account.";
                    return Page();
                }
            }

            // Update user information
            var updatedUser = new User
            {
                FirstName = Profile.FirstName,
                LastName = Profile.LastName,
                Email = Profile.Email,
                Role = currentUser.Role, // Keep the same role
                Password = !string.IsNullOrEmpty(Profile.NewPassword) 
                    ? _authService.HashPassword(Profile.NewPassword) 
                    : currentUser.Password
            };

            var success = await _authService.UpdateUserAsync(userEmail, updatedUser);
            if (success)
            {
                SuccessMessage = "Profile updated successfully!";
                
                // If email changed, need to update claims and re-authenticate
                if (!Profile.Email.Equals(currentUser.Email, StringComparison.OrdinalIgnoreCase))
                {
                    // For now, redirect to login to re-authenticate with new email
                    return RedirectToPage("/Account/Login");
                }
                
                // Clear password fields after successful update
                Profile.CurrentPassword = null;
                Profile.NewPassword = null;
                Profile.ConfirmNewPassword = null;
            }
            else
            {
                ErrorMessage = "Failed to update profile. Please try again.";
            }

            return Page();
        }
    }
}