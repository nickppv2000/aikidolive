using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AikidoLive.Services.Blog;
using AikidoLive.DataModels;
using System.Security.Claims;

namespace AikidoLive.Pages.BlogManagement
{
    public class PostModel : PageModel
    {
        private readonly IBlogService _blogService;

        public BlogPost? BlogPost { get; set; }
        public bool CanEdit { get; set; }
        public bool IsAppreciated { get; set; }
        
        [BindProperty]
        public AddCommentModel NewComment { get; set; } = new AddCommentModel();
        
        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";

        public PostModel(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            BlogPost = await _blogService.GetBlogPostByIdAsync(id);
            
            if (BlogPost == null)
            {
                return NotFound();
            }

            // Increment view count
            await _blogService.IncrementViewCountAsync(id);

            // Check if current user can edit this post
            var currentUserEmail = User?.Identity?.Name ?? "";
            CanEdit = User?.Identity?.IsAuthenticated == true && 
                     BlogPost.AuthorEmail.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase);

            // Check if current user has appreciated this post
            IsAppreciated = User?.Identity?.IsAuthenticated == true && 
                           BlogPost.AppreciatedBy.Contains(currentUserEmail);

            NewComment.BlogPostId = id;

            return Page();
        }

        public async Task<IActionResult> OnPostAddCommentAsync(string id)
        {
            if (!User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid || string.IsNullOrEmpty(id))
            {
                await LoadPostData(id);
                return Page();
            }

            try
            {
                var userEmail = User?.Identity?.Name ?? "";
                var userName = User?.FindFirst(ClaimTypes.GivenName)?.Value + " " + User?.FindFirst(ClaimTypes.Surname)?.Value;
                if (string.IsNullOrEmpty(userName?.Trim()))
                {
                    userName = userEmail.Split('@')[0];
                }

                var comment = new Comment
                {
                    Content = NewComment.Content,
                    AuthorEmail = userEmail,
                    AuthorName = userName.Trim(),
                    BlogPostId = id
                };

                var success = await _blogService.AddCommentAsync(id, comment);

                if (success)
                {
                    SuccessMessage = "Comment added successfully!";
                    NewComment = new AddCommentModel { BlogPostId = id };
                    ModelState.Clear();
                }
                else
                {
                    ErrorMessage = "Failed to add comment. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while adding the comment: " + ex.Message;
            }

            await LoadPostData(id);
            return Page();
        }

        public async Task<IActionResult> OnPostToggleAppreciationAsync(string id)
        {
            if (!User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Account/Login");
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var userEmail = User?.Identity?.Name ?? "";
                await _blogService.ToggleAppreciationAsync(id, userEmail);
            }
            catch (Exception)
            {
                // Silently fail for appreciation toggle
            }

            return RedirectToPage("/Blog/Post/" + id);
        }

        private async Task LoadPostData(string id)
        {
            BlogPost = await _blogService.GetBlogPostByIdAsync(id);
            
            if (BlogPost != null)
            {
                var currentUserEmail = User?.Identity?.Name ?? "";
                CanEdit = User?.Identity?.IsAuthenticated == true && 
                         BlogPost.AuthorEmail.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase);
                IsAppreciated = User?.Identity?.IsAuthenticated == true && 
                               BlogPost.AppreciatedBy.Contains(currentUserEmail);
            }
        }
    }
}