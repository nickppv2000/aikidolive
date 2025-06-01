using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AikidoLive.Pages.Account
{
    public class EmailSentModel : PageModel
    {
        public string Message { get; set; } = "";

        public void OnGet()
        {
            Message = TempData["Message"]?.ToString() ?? "Please check your email for further instructions.";
        }
    }
}