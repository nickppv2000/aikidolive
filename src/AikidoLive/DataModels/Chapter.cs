using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AikidoLive.DataModels
{
    public class Chapter
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Source { get; set; }
        
        Chapter()
        {
            Description = "";
            Name = "";
            Url = "";
            Source = "";
        }

        Chapter(string description, string name, string url, string source)
        {
            Description = description;
            Name = name;
            Url = url;
            Source = source;
        }
    }
}