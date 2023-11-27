using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AikidoLive.DataModels
{
   
    public class Track
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Source { get; set; }

        Track()
        {
            Description = "";
            Name = "";
            Url = "";
            Source = "";
        }

        Track(string description, string name, string url, string source)
        {
            Description = description;
            Name = name;
            Url = url;
            Source = source;
        }        
    }
}