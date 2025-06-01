using AikidoLive.Services.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AikidoLive.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ConfirmEmailModel> _logger;

        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }

        public ConfirmEmailModel(IAuthService authService, ILogger<ConfirmEmailModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Message = "Invalid confirmation link.";
                IsSuccess = false;
                return Page();
            }

            try
            {
                var result = await _authService.ConfirmEmailAsync(token);
                
                if (result)
                {
                    Message = "Your email has been confirmed successfully! You can now log in to your account.";
                    IsSuccess = true;
                    _logger.LogInformation($"Email confirmed successfully for token: {token}");
                }
                else
                {
                    Message = "Email confirmation failed. The link may be invalid or expired.";
                    IsSuccess = false;
                    _logger.LogWarning($"Email confirmation failed for token: {token}");
                }
            }
            catch (System.Exception ex)
            {
                Message = "An error occurred while confirming your email. Please try again.";
                IsSuccess = false;
                _logger.LogError(ex, $"Error confirming email for token: {token}");
            }

            return Page();
        }
    }
}