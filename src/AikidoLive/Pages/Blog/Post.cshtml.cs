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
        public bool CanDelete { get; set; }
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
            var currentUserRole = User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
            
            CanEdit = User?.Identity?.IsAuthenticated == true && 
                     BlogPost.AuthorEmail.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase);
            
            // Admin users can delete any post, regular users can only delete their own posts
            CanDelete = User?.Identity?.IsAuthenticated == true && 
                       (currentUserRole.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                        BlogPost.AuthorEmail.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase));

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
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "Please log in to add a comment" });
                }
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid || string.IsNullOrEmpty(id))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return new JsonResult(new { success = false, message = string.Join(", ", errors) });
                }
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
                    // For AJAX requests, return JSON response with the new comment data
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        var currentUserRole = User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
                        var canDeleteComment = currentUserRole.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                                             comment.AuthorEmail.Equals(userEmail, StringComparison.OrdinalIgnoreCase);
                        
                        return new JsonResult(new 
                        { 
                            success = true, 
                            message = "Comment added successfully!",
                            comment = new
                            {
                                id = comment.Id,
                                content = comment.Content,
                                authorName = comment.AuthorName,
                                authorEmail = comment.AuthorEmail,
                                createdAt = comment.CreatedAt.ToString("MMM dd, yyyy 'at' h:mm tt"),
                                canDelete = canDeleteComment
                            }
                        });
                    }
                    
                    SuccessMessage = "Comment added successfully!";
                    NewComment = new AddCommentModel { BlogPostId = id };
                    ModelState.Clear();
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return new JsonResult(new { success = false, message = "Failed to add comment. Please try again." });
                    }
                    ErrorMessage = "Failed to add comment. Please try again.";
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "An error occurred while adding the comment: " + ex.Message });
                }
                ErrorMessage = "An error occurred while adding the comment: " + ex.Message;
            }

            await LoadPostData(id);
            return Page();
        }

        public async Task<IActionResult> OnPostToggleAppreciationAsync(string id)
        {
            if (!User?.Identity?.IsAuthenticated == true)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "Please log in to appreciate this post" });
                }
                return RedirectToPage("/Account/Login");
            }

            if (string.IsNullOrEmpty(id))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "Invalid post ID" });
                }
                return NotFound();
            }

            try
            {
                var userEmail = User?.Identity?.Name ?? "";
                await _blogService.ToggleAppreciationAsync(id, userEmail);
                
                // Get updated post data for AJAX response
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var updatedPost = await _blogService.GetBlogPostByIdAsync(id);
                    if (updatedPost != null)
                    {
                        var isAppreciated = updatedPost.AppreciatedBy.Contains(userEmail);
                        return new JsonResult(new 
                        { 
                            success = true, 
                            isAppreciated = isAppreciated,
                            appreciationCount = updatedPost.AppreciatedBy.Count
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "Failed to update appreciation: " + ex.Message });
                }
                // Silently fail for appreciation toggle on regular requests
            }

            return RedirectToPage("/Blog/Post", new { id = id });
        }

        public async Task<IActionResult> OnPostDeletePostAsync(string id)
        {
            if (!User?.Identity?.IsAuthenticated == true || string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Check if user has permission to delete
            var blogPost = await _blogService.GetBlogPostByIdAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }

            var currentUserEmail = User?.Identity?.Name ?? "";
            var currentUserRole = User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
            
            bool canDelete = currentUserRole.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                           blogPost.AuthorEmail.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase);
            
            if (!canDelete)
            {
                return Forbid();
            }

            try
            {
                await _blogService.DeleteBlogPostAsync(id);
                return RedirectToPage("/Blog");
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to delete blog post. Please try again.";
                await LoadPostData(id);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(string id, string commentId)
        {
            if (!User?.Identity?.IsAuthenticated == true || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(commentId))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "Unauthorized or invalid request" });
                }
                return NotFound();
            }

            // Check if user has permission to delete (admin or comment author)
            var blogPost = await _blogService.GetBlogPostByIdAsync(id);
            if (blogPost == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "Blog post not found" });
                }
                return NotFound();
            }

            var comment = blogPost.Comments.FirstOrDefault(c => c.Id == commentId);
            if (comment == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "Comment not found" });
                }
                return NotFound();
            }

            var currentUserEmail = User?.Identity?.Name ?? "";
            var currentUserRole = User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
            
            bool canDelete = currentUserRole.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                           comment.AuthorEmail.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase);
            
            if (!canDelete)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "Not authorized to delete this comment" });
                }
                return Forbid();
            }

            try
            {
                await _blogService.DeleteCommentAsync(id, commentId);
                
                // For AJAX requests, return JSON response
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = true, message = "Comment deleted successfully" });
                }
                
                SuccessMessage = "Comment deleted successfully.";
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "Failed to delete comment: " + ex.Message });
                }
                ErrorMessage = "Failed to delete comment. Please try again.";
            }

            await LoadPostData(id);
            return Page();
        }

        private async Task LoadPostData(string id)
        {
            BlogPost = await _blogService.GetBlogPostByIdAsync(id);
            
            if (BlogPost != null)
            {
                var currentUserEmail = User?.Identity?.Name ?? "";
                var currentUserRole = User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
                
                CanEdit = User?.Identity?.IsAuthenticated == true && 
                         BlogPost.AuthorEmail.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase);
                         
                CanDelete = User?.Identity?.IsAuthenticated == true && 
                           (currentUserRole.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                            BlogPost.AuthorEmail.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase));
                            
                IsAppreciated = User?.Identity?.IsAuthenticated == true && 
                               BlogPost.AppreciatedBy.Contains(currentUserEmail);
            }
        }
    }
}