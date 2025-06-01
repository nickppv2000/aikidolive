using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using AikidoLive.Services.Blog;
using AikidoLive.DataModels;

namespace AikidoLive.Pages.BlogManagement
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IBlogService _blogService;

        [BindProperty]
        public CreateBlogPostModel BlogPost { get; set; } = new CreateBlogPostModel();
        
        public string BlogPostId { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";

        public EditModel(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var existingPost = await _blogService.GetBlogPostByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound();
            }

            // Check if current user can edit this post
            var currentUserEmail = User?.Identity?.Name ?? "";
            if (!existingPost.AuthorEmail.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            // Populate the form with existing data
            BlogPostId = id;
            BlogPost = new CreateBlogPostModel
            {
                Title = existingPost.Title,
                Content = existingPost.Content,
                Tags = string.Join(", ", existingPost.Tags),
                IsPublished = existingPost.IsPublished
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                BlogPostId = id;
                return Page();
            }

            try
            {
                var existingPost = await _blogService.GetBlogPostByIdAsync(id);
                if (existingPost == null)
                {
                    return NotFound();
                }

                // Check if current user can edit this post
                var currentUserEmail = User?.Identity?.Name ?? "";
                if (!existingPost.AuthorEmail.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }

                // Update the existing post
                existingPost.Title = BlogPost.Title;
                existingPost.Content = BlogPost.Content;
                existingPost.IsPublished = BlogPost.IsPublished;
                existingPost.UpdatedAt = DateTime.UtcNow;
                existingPost.Tags = string.IsNullOrEmpty(BlogPost.Tags) ? 
                    new List<string>() : 
                    BlogPost.Tags.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();

                var success = await _blogService.UpdateBlogPostAsync(existingPost);

                if (success)
                {
                    return RedirectToPage("/Blog/Post", new { id = id });
                }
                else
                {
                    ErrorMessage = "Failed to update blog post. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while updating the blog post: " + ex.Message;
            }

            BlogPostId = id;
            return Page();
        }
    }
}