using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AikidoLive.DataModels
{
    public class PlaylistsContent
    {
        public string PlaylistName { get; set; }
        public List<Track> Tracks { get; set; }

        public PlaylistsContent()
        {
            PlaylistName = "";
            Tracks = new List<Track>();
        }

        public PlaylistsContent(string playlistName, List<Track> tracks)
        {
            PlaylistName = playlistName;
            Tracks = tracks;
        }
    }
}