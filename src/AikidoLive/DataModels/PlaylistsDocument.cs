using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AikidoLive.DataModels
{
    public class PlaylistsDocument
    {
        public string Id { get; set; }
        public List<PlaylistsContent> PlaylistsContents { get; set; }

        public PlaylistsDocument()
        {
            Id = "";
            PlaylistsContents = new List<PlaylistsContent>();
        }

        public PlaylistsDocument(string id, List<PlaylistsContent> playlistsContent)
        {
            Id = id;
            PlaylistsContents = playlistsContent;
        }
    }
}