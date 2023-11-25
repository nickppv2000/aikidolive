using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AikidoLive.Services.DBConnector;
using AikidoLive.DataModels;

namespace AikidoLive.Pages
{
    public class LibraryView : PageModel
    {
        private readonly DBServiceConnector _dbServiceConnector;
        private readonly ILogger<LibraryView> _logger;
        private readonly IConfiguration _configuration;
        // This property will hold the data for the view
        public List<string> Databases { get; private set; }
        public List<LibraryDocument> _libDocuments;

        public LibraryView(ILogger<LibraryView> logger, DBServiceConnector dbServiceConnector, IConfiguration configuration)
        {
            _logger = logger;
            _dbServiceConnector = dbServiceConnector;
            _configuration = configuration;
            Databases = new List<string>();
            _libDocuments = new List<LibraryDocument>();
        }

        public async Task OnGetAsync()
        {
            var databases = await DBServiceConnector.CreateAsync(_configuration);
            //Databases = databases.GetDatabasesList();
            _libDocuments = await databases.GetLibraryTitles();

            if (null != _libDocuments)
            {
                foreach (var docs in _libDocuments)
                {
                    foreach (var content in docs.LibraryContents)
                    {
                        foreach (var chapter in content.Chapters)
                        {
                            if ("vimeo" == chapter.Source)
                            {
                                chapter.Url = "https://player.vimeo.com/video/" + chapter.Url;
                            }
                            //chapter.Name;
                        }
                    }
                }
            }
        }
    }
}