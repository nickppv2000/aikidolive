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
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;



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

        [BindProperty]
        public AddTrackInputModel AddTrackInput { get; set; } = new AddTrackInputModel();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public bool IsAdmin => User?.IsInRole("Admin") == true;
        
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
            _playlistsDocuments = await _dbServiceConnector.GetPlaylists();

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

        public async Task<IActionResult> OnPostAddTrackAsync()
        {
            if (!IsAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            try
            {
                _playlistsDocuments = await _dbServiceConnector.GetPlaylists();
                
                if (_playlistsDocuments == null || !_playlistsDocuments.Any())
                {
                    ErrorMessage = "No playlists found in the database.";
                    await OnGetAsync();
                    return Page();
                }

                var playlistDoc = _playlistsDocuments.FirstOrDefault();
                if (playlistDoc?.PlaylistsContents == null)
                {
                    ErrorMessage = "Playlist document structure is invalid.";
                    await OnGetAsync();
                    return Page();
                }

                var targetPlaylist = playlistDoc.PlaylistsContents
                    .FirstOrDefault(p => p.PlaylistName == AddTrackInput.PlaylistName);

                if (targetPlaylist == null)
                {
                    ErrorMessage = $"Playlist '{AddTrackInput.PlaylistName}' not found.";
                    await OnGetAsync();
                    return Page();
                }

                var newTrack = new Track
                {
                    Name = AddTrackInput.TrackName,
                    Description = AddTrackInput.Description ?? "",
                    Url = ProcessVideoUrl(AddTrackInput.Url, AddTrackInput.Source),
                    Source = AddTrackInput.Source
                };

                targetPlaylist.Tracks.Add(newTrack);

                bool updateSuccess = await _dbServiceConnector.UpdatePlaylists(playlistDoc);
                
                if (updateSuccess)
                {
                    SuccessMessage = $"Track '{newTrack.Name}' added successfully to playlist '{targetPlaylist.PlaylistName}'.";
                    // Clear the form
                    AddTrackInput = new AddTrackInputModel();
                }
                else
                {
                    ErrorMessage = "Failed to update the playlist in the database.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding track to playlist");
                ErrorMessage = "An error occurred while adding the track.";
            }

            await OnGetAsync();
            return Page();
        }

        private string ProcessVideoUrl(string url, string source)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            // For Vimeo, extract just the video ID if a full URL is provided
            if (source.ToLower() == "vimeo")
            {
                // If it's already just a number, return as-is
                if (url.All(char.IsDigit))
                    return url;

                // Extract video ID from various Vimeo URL formats
                var vimeoPatterns = new[]
                {
                    @"vimeo\.com/(\d+)",
                    @"player\.vimeo\.com/video/(\d+)"
                };

                foreach (var pattern in vimeoPatterns)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(url, pattern);
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }

            return url;
        }

        public class AddTrackInputModel
        {
            [Required(ErrorMessage = "Playlist name is required")]
            public string PlaylistName { get; set; } = "";

            [Required(ErrorMessage = "Track name is required")]
            public string TrackName { get; set; } = "";

            public string? Description { get; set; }

            [Required(ErrorMessage = "URL is required")]
            [Url(ErrorMessage = "Please enter a valid URL")]
            public string Url { get; set; } = "";

            [Required(ErrorMessage = "Source is required")]
            public string Source { get; set; } = "vimeo";
        }

        
    }
}