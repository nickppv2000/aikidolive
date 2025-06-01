using System.Collections.Generic;
using Newtonsoft.Json;

namespace AikidoLive.DataModels
{
    public class BlogDocument
    {
        [JsonProperty("id")]
        public string id { get; set; }
        
        [JsonProperty("tenantid")]
        public string tenantid { get; set; }
        
        [JsonProperty("blogPosts")]
        public List<BlogPost> BlogPosts { get; set; }

        public BlogDocument()
        {
            id = "blog";
            tenantid = "aikido-org";
            BlogPosts = new List<BlogPost>();
        }
    }
}