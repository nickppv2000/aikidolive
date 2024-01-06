using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AikidoLive.Services.DBConnector;
using AikidoLive.DataModels;
using AikidoLive.Services;

namespace AikidoLive.Pages
{
    public class Login : PageModel
    {
        private readonly ILogger<Login> _logger;
        private readonly UserService _userService;
        public bool IsAuthenticated { get; set; }

        public Login(ILogger<Login> logger, UserService userService)
        {
            IsAuthenticated = false;
            _logger = logger;
            _userService = userService;
             Input = new InputModel(); 
        }

        [BindProperty]
        public InputModel Input { get; set; } 
        public class InputModel 
        {
            public string Email { get; set; }
            public string Password { get; set; }

            public InputModel()
            {
                Email = "";
                Password = "";
            }

            public InputModel(string email, string password)
            {
                Email = email;
                Password = password;
            }
        }

        //public async Task OnGetAsync()
        //{
            //_libUserList = await _dbServiceConnector.GetUsers();
        //}

        public async Task<IActionResult> OnPostAsync()
        {
            IActionResult result = StatusCode(404);
            var signinresult = await _userService.LoginUser(Input.Email, Input.Password);

            if (signinresult.Succeeded)
            {
                IsAuthenticated = true;
                result = RedirectToPage("/Index");
            }
            else
            {
                // Show error
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                result = Page();
            }

            /*  var user = await _userService.GetUserByEmail(Input.Email);
            if (user != null && user.Password == Input.Password)
            {
                IsAuthenticated = true;
                // Authenticate user
                result = RedirectToPage("/Index");
            }
            else
            {
                // Show error
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                result = Page();
            }*/

            return result;
        }
    }
}