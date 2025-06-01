using AikidoLive.Services.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace AikidoLive.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ResetPasswordModel> _logger;

        [BindProperty]
        public ResetPasswordInputModel Input { get; set; } = new ResetPasswordInputModel();

        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }

        public ResetPasswordModel(IAuthService authService, ILogger<ResetPasswordModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public IActionResult OnGet(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Message = "Invalid password reset link.";
                IsSuccess = false;
                return Page();
            }

            Input.Token = token;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _authService.ResetPasswordAsync(Input.Token, Input.NewPassword);
                
                if (result)
                {
                    Message = "Your password has been reset successfully! You can now log in with your new password.";
                    IsSuccess = true;
                    _logger.LogInformation($"Password reset successfully for token: {Input.Token}");
                }
                else
                {
                    Message = "Password reset failed. The link may be invalid or expired.";
                    IsSuccess = false;
                    _logger.LogWarning($"Password reset failed for token: {Input.Token}");
                }
            }
            catch (System.Exception ex)
            {
                Message = "An error occurred while resetting your password. Please try again.";
                IsSuccess = false;
                _logger.LogError(ex, $"Error resetting password for token: {Input.Token}");
            }

            return Page();
        }

        public class ResetPasswordInputModel
        {
            [Required]
            public string Token { get; set; } = "";

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = "";

            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = "";
        }
    }
}