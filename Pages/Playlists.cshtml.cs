using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AikidoLive.Services;
using AikidoLive.Services.DBConnector;
using AikidoLive.DataModels;



namespace AikidoLive.Pages
{
    public class Playlists : PageModel
    {

        private readonly DBServiceConnector _dbServiceConnector;
        private readonly ILogger<Playlists> _logger;
        private readonly IConfiguration _configuration;
        // This property will hold the data for the view
        public List<string> Databases { get; private set; }
        public List<PlaylistsDocument> _playlistsDocuments;
        
        public Playlists(ILogger<Playlists> logger, DBServiceConnector dbServiceConnector, IConfiguration configuration)
        {
            _logger = logger;
            _dbServiceConnector = dbServiceConnector;
            _configuration = configuration;
            Databases = new List<string>();
            _playlistsDocuments = new List<PlaylistsDocument>();
        }


        public async Task OnGetAsync()
        {
            var databases = await DBServiceConnector.CreateAsync(_configuration);
            //Databases = databases.GetDatabasesList();
            _playlistsDocuments = await databases.GetPlaylists();

            if (null != _playlistsDocuments)
            {
                foreach (var docs in _playlistsDocuments)
                {
                    foreach (var content in docs.PlaylistsContents)
                    {
                        foreach (var track in content.Tracks)
                        {
                            if ("vimeo" == track.Source)
                            {
                                track.Url = "https://player.vimeo.com/video/" + track.Url;
                            }
                        }
                    }
                }
            }
        }

        
    }
}