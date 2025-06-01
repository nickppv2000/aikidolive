using AikidoLive.Services.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace AikidoLive.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ForgotPasswordModel> _logger;

        [BindProperty]
        public ForgotPasswordInputModel Input { get; set; } = new ForgotPasswordInputModel();

        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }

        public ForgotPasswordModel(IAuthService authService, ILogger<ForgotPasswordModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _authService.SendPasswordResetEmailAsync(Input.Email);
                
                if (result)
                {
                    _logger.LogInformation($"Password reset email sent to {Input.Email}");
                    TempData["Message"] = "If an account with that email exists, we've sent you a password reset link.";
                    return RedirectToPage("/Account/EmailSent");
                }
                else
                {
                    // Don't reveal whether the email exists or not for security
                    TempData["Message"] = "If an account with that email exists, we've sent you a password reset link.";
                    return RedirectToPage("/Account/EmailSent");
                }
            }
            catch (System.Exception ex)
            {
                Message = "An error occurred while processing your request. Please try again.";
                IsSuccess = false;
                _logger.LogError(ex, $"Error sending password reset email to {Input.Email}");
                return Page();
            }
        }

        public class ForgotPasswordInputModel
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            public string Email { get; set; } = "";
        }
    }
}