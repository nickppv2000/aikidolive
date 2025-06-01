using System.ComponentModel.DataAnnotations;

namespace AikidoLive.DataModels
{
    public class CreateBlogPostModel
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Title { get; set; }
        
        [Required]
        [StringLength(50000, MinimumLength = 50)]
        public string Content { get; set; }
        
        public string Tags { get; set; } // Comma-separated tags
        
        public bool IsPublished { get; set; }

        public CreateBlogPostModel()
        {
            Title = "";
            Content = "";
            Tags = "";
            IsPublished = false;
        }
    }
}