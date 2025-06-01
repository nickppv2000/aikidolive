using System;

namespace AikidoLive.DataModels
{
    public class Comment
    {
        public string Id { get; set; }
        public string BlogPostId { get; set; }
        public string Content { get; set; }
        public string AuthorEmail { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsApproved { get; set; }

        public Comment()
        {
            Id = Guid.NewGuid().ToString();
            BlogPostId = "";
            Content = "";
            AuthorEmail = "";
            AuthorName = "";
            CreatedAt = DateTime.UtcNow;
            IsApproved = true; // Auto-approve for now, can be changed later
        }
    }
}