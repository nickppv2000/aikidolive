using System.ComponentModel.DataAnnotations;

namespace AikidoLive.DataModels
{
    public class AddCommentModel
    {
        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Content { get; set; }
        
        public string BlogPostId { get; set; }

        public AddCommentModel()
        {
            Content = "";
            BlogPostId = "";
        }
    }
}