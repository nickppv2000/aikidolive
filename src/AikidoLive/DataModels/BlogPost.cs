using System;
using System.Collections.Generic;

namespace AikidoLive.DataModels
{
    public class BlogPost
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorEmail { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsPublished { get; set; }
        public List<string> Tags { get; set; }
        public List<Comment> Comments { get; set; }
        public List<string> AppreciatedBy { get; set; } // List of user emails who appreciated
        public int ViewCount { get; set; }

        public BlogPost()
        {
            Id = Guid.NewGuid().ToString();
            Title = "";
            Content = "";
            AuthorEmail = "";
            AuthorName = "";
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsPublished = false;
            Tags = new List<string>();
            Comments = new List<Comment>();
            AppreciatedBy = new List<string>();
            ViewCount = 0;
        }
    }
}