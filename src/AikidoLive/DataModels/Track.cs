using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AikidoLive.DataModels
{
   
    public class Track
    {
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("source")]
        public string Source { get; set; }

        public Track()
        {
            Description = "";
            Name = "";
            Url = "";
            Source = "";
        }

        public Track(string description, string name, string url, string source)
        {
            Description = description;
            Name = name;
            Url = url;
            Source = source;
        }        
    }
}