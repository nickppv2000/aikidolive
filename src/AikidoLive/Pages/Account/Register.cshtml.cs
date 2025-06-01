
using AikidoLive.DataModels;
using AikidoLive.Services.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;


namespace AikidoLive.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]

        public RegisterInputModel RegisterInput { get; set; } = new RegisterInputModel();

        public string ErrorMessage { get; set; }


        public RegisterModel(IAuthService authService, ILogger<RegisterModel> logger)
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

            var user = await _authService.RegisterUserAsync(new DataModels.RegisterModel
            {
                FirstName = RegisterInput.FirstName,
                LastName = RegisterInput.LastName,
                Email = RegisterInput.Email,
                Password = RegisterInput.Password
            });

            if (user == null)
            {
                ErrorMessage = "Email is already in use.";
                return Page();
            }

            _logger.LogInformation($"User {RegisterInput.Email} registered successfully. Email confirmation required.");
            
            // Redirect to a confirmation page instead of login
            TempData["Message"] = "Registration successful! Please check your email for a confirmation link to activate your account.";
            return RedirectToPage("/Account/EmailSent");
        }

        public class RegisterInputModel
        {
            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

        }
    }
}