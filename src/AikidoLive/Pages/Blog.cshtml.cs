using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AikidoLive.Services.Blog;
using AikidoLive.DataModels;

namespace AikidoLive.Pages
{
    public class BlogIndex : PageModel
    {
        private readonly ILogger<BlogIndex> _logger;
        private readonly IBlogService _blogService;

        public List<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

        public BlogIndex(ILogger<BlogIndex> logger, IBlogService blogService)
        {
            _logger = logger;
            _blogService = blogService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                BlogPosts = await _blogService.GetPublishedBlogPostsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading blog posts");
                BlogPosts = new List<BlogPost>();
            }
        }
    }
}