using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AikidoLive.Pages
{
    public class library_titles : PageModel
    {
        private readonly ILogger<library_titles> _logger;

        public library_titles(ILogger<library_titles> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}