using AikidoLive.DataModels;
using AikidoLive.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AikidoLive.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public LoginInputModel LoginInput { get; set; } = new LoginInputModel();

        public string ErrorMessage { get; set; }

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
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

            // First check if user exists and email is confirmed
            var userByEmail = await _authService.GetUserByEmailAsync(LoginInput.Email);
            if (userByEmail != null && !userByEmail.IsEmailConfirmed)
            {
                ErrorMessage = "Please confirm your email address before logging in. Check your email for a confirmation link.";
                return Page();
            }

            var user = await _authService.AuthenticateAsync(LoginInput.Email, LoginInput.Password);

            if (user == null)
            {
                ErrorMessage = "Invalid email or password";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = LoginInput.RememberMe
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation($"User {user.Email} logged in successfully");

            return RedirectToPage("/Index");
        }

        public class LoginInputModel
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }
    }
}