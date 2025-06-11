using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AikidoLive.DataModels
{
    public class PlaylistsDocument
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("playlistsContents")]
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