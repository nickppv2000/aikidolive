using AikidoLive.DataModels;
using AikidoLive.Services.DBConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AikidoLive.Services.Blog
{
    public interface IBlogService
    {
        Task<List<BlogPost>> GetPublishedBlogPostsAsync();
        Task<List<BlogPost>> GetBlogPostsByAuthorAsync(string authorEmail);
        Task<BlogPost?> GetBlogPostByIdAsync(string postId);
        Task<bool> CreateBlogPostAsync(BlogPost blogPost);
        Task<bool> UpdateBlogPostAsync(BlogPost blogPost);
        Task<bool> DeleteBlogPostAsync(string postId);
        Task<bool> AddCommentAsync(string postId, Comment comment);
        Task<bool> ToggleAppreciationAsync(string postId, string userEmail);
        Task<bool> IncrementViewCountAsync(string postId);
    }

    public class BlogService : IBlogService
    {
        private readonly DBServiceConnector _dbServiceConnector;

        public BlogService(DBServiceConnector dbServiceConnector)
        {
            _dbServiceConnector = dbServiceConnector;
        }

        public async Task<List<BlogPost>> GetPublishedBlogPostsAsync()
        {
            var blogDocuments = await _dbServiceConnector.GetBlogPosts();
            if (blogDocuments == null || !blogDocuments.Any())
                return new List<BlogPost>();

            var blogDocument = blogDocuments.FirstOrDefault();
            if (blogDocument?.BlogPosts == null)
                return new List<BlogPost>();

            return blogDocument.BlogPosts
                .Where(post => post.IsPublished)
                .OrderByDescending(post => post.CreatedAt)
                .ToList();
        }

        public async Task<List<BlogPost>> GetBlogPostsByAuthorAsync(string authorEmail)
        {
            var blogDocuments = await _dbServiceConnector.GetBlogPosts();
            if (blogDocuments == null || !blogDocuments.Any())
                return new List<BlogPost>();

            var blogDocument = blogDocuments.FirstOrDefault();
            if (blogDocument?.BlogPosts == null)
                return new List<BlogPost>();

            return blogDocument.BlogPosts
                .Where(post => post.AuthorEmail.Equals(authorEmail, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(post => post.CreatedAt)
                .ToList();
        }

        public async Task<BlogPost?> GetBlogPostByIdAsync(string postId)
        {
            var blogDocuments = await _dbServiceConnector.GetBlogPosts();
            if (blogDocuments == null || !blogDocuments.Any())
                return null;

            var blogDocument = blogDocuments.FirstOrDefault();
            if (blogDocument?.BlogPosts == null)
                return null;

            return blogDocument.BlogPosts.FirstOrDefault(post => post.Id == postId);
        }

        public async Task<bool> CreateBlogPostAsync(BlogPost blogPost)
        {
            var blogDocuments = await _dbServiceConnector.GetBlogPosts();
            BlogDocument blogDocument;

            if (blogDocuments == null || !blogDocuments.Any())
            {
                blogDocument = new BlogDocument();
            }
            else
            {
                blogDocument = blogDocuments.FirstOrDefault() ?? new BlogDocument();
            }

            blogDocument.BlogPosts.Add(blogPost);
            return await _dbServiceConnector.UpdateBlogDocument(blogDocument);
        }

        public async Task<bool> UpdateBlogPostAsync(BlogPost updatedBlogPost)
        {
            var blogDocuments = await _dbServiceConnector.GetBlogPosts();
            if (blogDocuments == null || !blogDocuments.Any())
                return false;

            var blogDocument = blogDocuments.FirstOrDefault();
            if (blogDocument?.BlogPosts == null)
                return false;

            var existingPost = blogDocument.BlogPosts.FirstOrDefault(post => post.Id == updatedBlogPost.Id);
            if (existingPost == null)
                return false;

            // Update the post
            var index = blogDocument.BlogPosts.IndexOf(existingPost);
            blogDocument.BlogPosts[index] = updatedBlogPost;
            
            return await _dbServiceConnector.UpdateBlogDocument(blogDocument);
        }

        public async Task<bool> DeleteBlogPostAsync(string postId)
        {
            var blogDocuments = await _dbServiceConnector.GetBlogPosts();
            if (blogDocuments == null || !blogDocuments.Any())
                return false;

            var blogDocument = blogDocuments.FirstOrDefault();
            if (blogDocument?.BlogPosts == null)
                return false;

            var postToRemove = blogDocument.BlogPosts.FirstOrDefault(post => post.Id == postId);
            if (postToRemove == null)
                return false;

            blogDocument.BlogPosts.Remove(postToRemove);
            return await _dbServiceConnector.UpdateBlogDocument(blogDocument);
        }

        public async Task<bool> AddCommentAsync(string postId, Comment comment)
        {
            var blogDocuments = await _dbServiceConnector.GetBlogPosts();
            if (blogDocuments == null || !blogDocuments.Any())
                return false;

            var blogDocument = blogDocuments.FirstOrDefault();
            if (blogDocument?.BlogPosts == null)
                return false;

            var post = blogDocument.BlogPosts.FirstOrDefault(p => p.Id == postId);
            if (post == null)
                return false;

            comment.BlogPostId = postId;
            post.Comments.Add(comment);

            return await _dbServiceConnector.UpdateBlogDocument(blogDocument);
        }

        public async Task<bool> ToggleAppreciationAsync(string postId, string userEmail)
        {
            var blogDocuments = await _dbServiceConnector.GetBlogPosts();
            if (blogDocuments == null || !blogDocuments.Any())
                return false;

            var blogDocument = blogDocuments.FirstOrDefault();
            if (blogDocument?.BlogPosts == null)
                return false;

            var post = blogDocument.BlogPosts.FirstOrDefault(p => p.Id == postId);
            if (post == null)
                return false;

            if (post.AppreciatedBy.Contains(userEmail))
            {
                post.AppreciatedBy.Remove(userEmail);
            }
            else
            {
                post.AppreciatedBy.Add(userEmail);
            }

            return await _dbServiceConnector.UpdateBlogDocument(blogDocument);
        }

        public async Task<bool> IncrementViewCountAsync(string postId)
        {
            var blogDocuments = await _dbServiceConnector.GetBlogPosts();
            if (blogDocuments == null || !blogDocuments.Any())
                return false;

            var blogDocument = blogDocuments.FirstOrDefault();
            if (blogDocument?.BlogPosts == null)
                return false;

            var post = blogDocument.BlogPosts.FirstOrDefault(p => p.Id == postId);
            if (post == null)
                return false;

            post.ViewCount++;
            return await _dbServiceConnector.UpdateBlogDocument(blogDocument);
        }
    }
}