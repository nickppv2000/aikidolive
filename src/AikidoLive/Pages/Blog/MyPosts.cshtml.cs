using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using AikidoLive.Services.Blog;
using AikidoLive.DataModels;

namespace AikidoLive.Pages.BlogManagement
{
    [Authorize]
    public class MyPostsModel : PageModel
    {
        private readonly IBlogService _blogService;

        public List<BlogPost> MyBlogPosts { get; set; } = new List<BlogPost>();
        public string ErrorMessage { get; set; } = "";

        public MyPostsModel(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var currentUserEmail = User?.Identity?.Name ?? "";
                MyBlogPosts = await _blogService.GetBlogPostsByAuthorAsync(currentUserEmail);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load your blog posts: " + ex.Message;
                MyBlogPosts = new List<BlogPost>();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                // Verify the post belongs to the current user
                var post = await _blogService.GetBlogPostByIdAsync(id);
                if (post == null)
                {
                    return NotFound();
                }

                var currentUserEmail = User?.Identity?.Name ?? "";
                if (!post.AuthorEmail.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }

                await _blogService.DeleteBlogPostAsync(id);
            }
            catch (Exception)
            {
                // Silently handle errors for delete operation
            }

            return RedirectToPage();
        }
    }
}