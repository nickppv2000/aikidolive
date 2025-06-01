using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using AikidoLive.Services.Blog;
using AikidoLive.DataModels;
using System.Security.Claims;

namespace AikidoLive.Pages.BlogManagement
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IBlogService _blogService;

        [BindProperty]
        public CreateBlogPostModel BlogPost { get; set; } = new CreateBlogPostModel();

        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";

        public CreateModel(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var userEmail = User?.Identity?.Name ?? "";
                var userName = User?.FindFirst(ClaimTypes.GivenName)?.Value + " " + User?.FindFirst(ClaimTypes.Surname)?.Value;
                if (string.IsNullOrEmpty(userName?.Trim()))
                {
                    userName = userEmail.Split('@')[0]; // Use email prefix if name not available
                }

                var blogPost = new BlogPost
                {
                    Title = BlogPost.Title,
                    Content = BlogPost.Content,
                    AuthorEmail = userEmail,
                    AuthorName = userName.Trim(),
                    IsPublished = BlogPost.IsPublished,
                    Tags = string.IsNullOrEmpty(BlogPost.Tags) ? 
                        new List<string>() : 
                        BlogPost.Tags.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList()
                };

                var success = await _blogService.CreateBlogPostAsync(blogPost);

                if (success)
                {
                    if (blogPost.IsPublished)
                    {
                        return RedirectToPage("/Blog/Post/" + blogPost.Id);
                    }
                    else
                    {
                        SuccessMessage = "Blog post saved as draft successfully!";
                        ModelState.Clear();
                        BlogPost = new CreateBlogPostModel();
                    }
                }
                else
                {
                    ErrorMessage = "Failed to create blog post. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while creating the blog post: " + ex.Message;
            }

            return Page();
        }
    }
}