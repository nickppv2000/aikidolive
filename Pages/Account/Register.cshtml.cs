using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AikidoLive.DataModels;
using AikidoLive.Services;
using System.ComponentModel.DataAnnotations;

namespace AikidoLive.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
        }

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
            if (ModelState.IsValid)
            {
                var user = new User 
                { 
                    FirstName = Input.FirstName, 
                    LastName = Input.LastName, 
                    Email = Input.Email, 
                    Password = Input.Password, // In a real app, you should hash the password
                    Role = "User" 
                };

                var result = await _authService.RegisterUser(user);
                if (result)
                {
                    _logger.LogInformation("User created a new account.");
                    return RedirectToPage("/Index");
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}