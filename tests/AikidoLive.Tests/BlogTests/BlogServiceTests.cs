using AikidoLive.DataModels;
using AikidoLive.Services.Blog;
using AikidoLive.Services.DBConnector;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AikidoLive.Tests.BlogTests
{
    public class BlogServiceTests
    {
        private readonly Mock<IDBServiceConnector> _mockDbConnector;
        private readonly BlogService _blogService;

        public BlogServiceTests()
        {
            _mockDbConnector = new Mock<IDBServiceConnector>();
            _blogService = new BlogService(_mockDbConnector.Object);
        }

        [Fact]
        public async Task CreateBlogPostAsync_WithValidPost_ShouldReturnTrue()
        {
            // Arrange
            var blogPost = new BlogPost
            {
                Title = "Test Aikido Post",
                Content = "This is a test post about Aikido philosophy.",
                AuthorEmail = "test@example.com",
                AuthorName = "Test Author",
                IsPublished = true
            };

            var existingBlogDocument = new BlogDocument();
            _mockDbConnector.Setup(db => db.GetBlogPosts())
                .ReturnsAsync(new List<BlogDocument> { existingBlogDocument });
            
            _mockDbConnector.Setup(db => db.UpdateBlogDocument(It.IsAny<BlogDocument>()))
                .ReturnsAsync(true);

            // Act
            var result = await _blogService.CreateBlogPostAsync(blogPost);

            // Assert
            Assert.True(result);
            _mockDbConnector.Verify(db => db.UpdateBlogDocument(It.IsAny<BlogDocument>()), Times.Once);
        }

        [Fact]
        public async Task GetPublishedBlogPostsAsync_ShouldReturnOnlyPublishedPosts()
        {
            // Arrange
            var publishedPost = new BlogPost
            {
                Id = "1",
                Title = "Published Post",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            var draftPost = new BlogPost
            {
                Id = "2", 
                Title = "Draft Post",
                IsPublished = false,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            };

            var blogDocument = new BlogDocument();
            blogDocument.BlogPosts.Add(publishedPost);
            blogDocument.BlogPosts.Add(draftPost);

            _mockDbConnector.Setup(db => db.GetBlogPosts())
                .ReturnsAsync(new List<BlogDocument> { blogDocument });

            // Act
            var result = await _blogService.GetPublishedBlogPostsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Published Post", result.First().Title);
            Assert.True(result.First().IsPublished);
        }

        [Fact]
        public async Task ToggleAppreciationAsync_ShouldAddUserToAppreciatedBy()
        {
            // Arrange
            var blogPost = new BlogPost
            {
                Id = "1",
                Title = "Test Post",
                AppreciatedBy = new List<string>()
            };

            var blogDocument = new BlogDocument();
            blogDocument.BlogPosts.Add(blogPost);

            _mockDbConnector.Setup(db => db.GetBlogPosts())
                .ReturnsAsync(new List<BlogDocument> { blogDocument });
            
            _mockDbConnector.Setup(db => db.UpdateBlogDocument(It.IsAny<BlogDocument>()))
                .ReturnsAsync(true);

            // Act
            var result = await _blogService.ToggleAppreciationAsync("1", "user@example.com");

            // Assert
            Assert.True(result);
            Assert.Contains("user@example.com", blogPost.AppreciatedBy);
        }

        [Fact]
        public async Task AddCommentAsync_ShouldAddCommentToPost()
        {
            // Arrange
            var blogPost = new BlogPost
            {
                Id = "1",
                Title = "Test Post",
                Comments = new List<Comment>()
            };

            var blogDocument = new BlogDocument();
            blogDocument.BlogPosts.Add(blogPost);

            var comment = new Comment
            {
                Content = "Great post!",
                AuthorEmail = "commenter@example.com",
                AuthorName = "Test Commenter"
            };

            _mockDbConnector.Setup(db => db.GetBlogPosts())
                .ReturnsAsync(new List<BlogDocument> { blogDocument });
            
            _mockDbConnector.Setup(db => db.UpdateBlogDocument(It.IsAny<BlogDocument>()))
                .ReturnsAsync(true);

            // Act
            var result = await _blogService.AddCommentAsync("1", comment);

            // Assert
            Assert.True(result);
            Assert.Single(blogPost.Comments);
            Assert.Equal("Great post!", blogPost.Comments.First().Content);
        }
    }
}