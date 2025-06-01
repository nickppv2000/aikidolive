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
using Xunit.Abstractions;

namespace AikidoLive.Tests.BlogTests
{
    /// <summary>
    /// Enhanced unit tests with scenarios that mimic real-world failure conditions
    /// to help diagnose blog post saving issues
    /// </summary>
    public class BlogServiceEnhancedTests
    {
        private readonly ITestOutputHelper _output;

        public BlogServiceEnhancedTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task CreateBlogPost_WithEmptyDatabasesDictionary_ShouldFail()
        {
            _output.WriteLine("Testing blog post creation when database discovery fails...");

            // Arrange
            var mockDbConnector = new Mock<IDBServiceConnector>();
            
            // Simulate the scenario where GetBlogPosts fails due to empty _databasesDictionary
            mockDbConnector.Setup(db => db.GetBlogPosts())
                .ThrowsAsync(new InvalidOperationException("Database discovery failed. No databases found in _databasesDictionary."));

            var blogService = new BlogService(mockDbConnector.Object);

            var testPost = new BlogPost
            {
                Title = "Test Post",
                Content = "Test content",
                AuthorEmail = "test@example.com",
                AuthorName = "Test Author",
                IsPublished = true
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => blogService.CreateBlogPostAsync(testPost)
            );

            _output.WriteLine($"Expected exception caught: {exception.Message}");
            Assert.Contains("Database discovery failed", exception.Message);
        }

        [Fact]
        public async Task CreateBlogPost_WithNetworkFailure_ShouldFail()
        {
            _output.WriteLine("Testing blog post creation with network connectivity issues...");

            // Arrange
            var mockDbConnector = new Mock<IDBServiceConnector>();
            
            // Simulate network connectivity issues
            mockDbConnector.Setup(db => db.GetBlogPosts())
                .ThrowsAsync(new System.Net.Http.HttpRequestException("Resource temporarily unavailable"));

            var blogService = new BlogService(mockDbConnector.Object);

            var testPost = new BlogPost
            {
                Title = "Test Post",
                Content = "Test content",
                AuthorEmail = "test@example.com",
                AuthorName = "Test Author",
                IsPublished = true
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Net.Http.HttpRequestException>(
                () => blogService.CreateBlogPostAsync(testPost)
            );

            _output.WriteLine($"Expected network exception caught: {exception.Message}");
            Assert.Contains("Resource temporarily unavailable", exception.Message);
        }

        [Fact]
        public async Task CreateBlogPost_WithUpdateFailure_ShouldReturnFalse()
        {
            _output.WriteLine("Testing blog post creation when database update fails...");

            // Arrange
            var mockDbConnector = new Mock<IDBServiceConnector>();
            
            // Simulate successful GetBlogPosts but failed UpdateBlogDocument
            var existingBlogDocument = new BlogDocument();
            mockDbConnector.Setup(db => db.GetBlogPosts())
                .ReturnsAsync(new List<BlogDocument> { existingBlogDocument });
            
            mockDbConnector.Setup(db => db.UpdateBlogDocument(It.IsAny<BlogDocument>()))
                .ReturnsAsync(false);

            var blogService = new BlogService(mockDbConnector.Object);

            var testPost = new BlogPost
            {
                Title = "Test Post",
                Content = "Test content",
                AuthorEmail = "test@example.com",
                AuthorName = "Test Author",
                IsPublished = true
            };

            // Act
            var result = await blogService.CreateBlogPostAsync(testPost);

            // Assert
            _output.WriteLine($"Blog post creation result: {result}");
            Assert.False(result);
            mockDbConnector.Verify(db => db.UpdateBlogDocument(It.IsAny<BlogDocument>()), Times.Once);
        }

        [Fact]
        public async Task CreateBlogPost_WithValidScenario_ShouldSucceed()
        {
            _output.WriteLine("Testing successful blog post creation...");

            // Arrange
            var mockDbConnector = new Mock<IDBServiceConnector>();
            
            var existingBlogDocument = new BlogDocument();
            mockDbConnector.Setup(db => db.GetBlogPosts())
                .ReturnsAsync(new List<BlogDocument> { existingBlogDocument });
            
            mockDbConnector.Setup(db => db.UpdateBlogDocument(It.IsAny<BlogDocument>()))
                .ReturnsAsync(true);

            var blogService = new BlogService(mockDbConnector.Object);

            var testPost = new BlogPost
            {
                Title = "Test Post",
                Content = "Test content",
                AuthorEmail = "test@example.com",
                AuthorName = "Test Author",
                IsPublished = true
            };

            // Act
            var result = await blogService.CreateBlogPostAsync(testPost);

            // Assert
            _output.WriteLine($"Blog post creation result: {result}");
            Assert.True(result);
            
            // Verify the post was added to the document
            mockDbConnector.Verify(db => db.UpdateBlogDocument(It.Is<BlogDocument>(doc => 
                doc.BlogPosts.Any(p => p.Title == testPost.Title))), Times.Once);
        }

        [Fact]
        public async Task CreateBlogPost_WhenNoBlogDocumentExists_ShouldCreateDocument()
        {
            _output.WriteLine("Testing blog post creation when no blog document exists...");

            // Arrange
            var mockDbConnector = new Mock<IDBServiceConnector>();
            
            // Simulate no existing blog documents
            mockDbConnector.Setup(db => db.GetBlogPosts())
                .ReturnsAsync(new List<BlogDocument>());
            
            mockDbConnector.Setup(db => db.UpdateBlogDocument(It.IsAny<BlogDocument>()))
                .ReturnsAsync(true);

            var blogService = new BlogService(mockDbConnector.Object);

            var testPost = new BlogPost
            {
                Title = "Test Post",
                Content = "Test content",
                AuthorEmail = "test@example.com",
                AuthorName = "Test Author",
                IsPublished = true
            };

            // Act
            var result = await blogService.CreateBlogPostAsync(testPost);

            // Assert
            _output.WriteLine($"Blog post creation result: {result}");
            Assert.True(result);
            
            // Verify a new document was created and updated
            mockDbConnector.Verify(db => db.UpdateBlogDocument(It.Is<BlogDocument>(doc => 
                doc.BlogPosts.Count == 1 && doc.BlogPosts[0].Title == testPost.Title)), Times.Once);
        }

        [Fact]
        public async Task GetBlogPosts_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            _output.WriteLine("Testing GetBlogPosts with empty database...");

            // Arrange
            var mockDbConnector = new Mock<IDBServiceConnector>();
            
            mockDbConnector.Setup(db => db.GetBlogPosts())
                .ReturnsAsync(new List<BlogDocument>());

            var blogService = new BlogService(mockDbConnector.Object);

            // Act
            var result = await blogService.GetPublishedBlogPostsAsync();

            // Assert
            _output.WriteLine($"Retrieved {result.Count} blog posts from empty database");
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateBlogPost_WithValidData_ShouldSucceed()
        {
            _output.WriteLine("Testing blog post update...");

            // Arrange
            var originalPost = new BlogPost
            {
                Id = "test-post-id",
                Title = "Original Title",
                Content = "Original content",
                AuthorEmail = "test@example.com",
                AuthorName = "Test Author",
                IsPublished = false
            };

            var blogDocument = new BlogDocument();
            blogDocument.BlogPosts.Add(originalPost);

            var mockDbConnector = new Mock<IDBServiceConnector>();
            
            mockDbConnector.Setup(db => db.GetBlogPosts())
                .ReturnsAsync(new List<BlogDocument> { blogDocument });
            
            mockDbConnector.Setup(db => db.UpdateBlogDocument(It.IsAny<BlogDocument>()))
                .ReturnsAsync(true);

            var blogService = new BlogService(mockDbConnector.Object);

            // Modify the post
            originalPost.Title = "Updated Title";
            originalPost.IsPublished = true;

            // Act
            var result = await blogService.UpdateBlogPostAsync(originalPost);

            // Assert
            _output.WriteLine($"Blog post update result: {result}");
            Assert.True(result);
            
            // Verify the document was updated
            mockDbConnector.Verify(db => db.UpdateBlogDocument(It.Is<BlogDocument>(doc => 
                doc.BlogPosts.Any(p => p.Id == originalPost.Id && p.Title == "Updated Title"))), Times.Once);
        }

        [Fact]
        public async Task CreateBlogPost_WithNullDocument_ShouldHandleGracefully()
        {
            _output.WriteLine("Testing blog post creation with null document scenario...");

            // Arrange
            var mockDbConnector = new Mock<IDBServiceConnector>();
            
            // Simulate null return from GetBlogPosts
            mockDbConnector.Setup(db => db.GetBlogPosts())
                .ReturnsAsync((List<BlogDocument>)null);
            
            mockDbConnector.Setup(db => db.UpdateBlogDocument(It.IsAny<BlogDocument>()))
                .ReturnsAsync(true);

            var blogService = new BlogService(mockDbConnector.Object);

            var testPost = new BlogPost
            {
                Title = "Test Post",
                Content = "Test content",
                AuthorEmail = "test@example.com",
                AuthorName = "Test Author",
                IsPublished = true
            };

            // Act
            var result = await blogService.CreateBlogPostAsync(testPost);

            // Assert
            _output.WriteLine($"Blog post creation result with null document: {result}");
            Assert.True(result);
            
            // Should create a new document when null is returned
            mockDbConnector.Verify(db => db.UpdateBlogDocument(It.IsAny<BlogDocument>()), Times.Once);
        }

        [Fact]
        public void BlogPost_DefaultConstructor_ShouldInitializeCorrectly()
        {
            _output.WriteLine("Testing BlogPost default initialization...");

            // Act
            var blogPost = new BlogPost();

            // Assert
            _output.WriteLine($"BlogPost initialized with ID: {blogPost.Id}");
            Assert.False(string.IsNullOrEmpty(blogPost.Id));
            Assert.NotNull(blogPost.Tags);
            Assert.NotNull(blogPost.Comments);
            Assert.NotNull(blogPost.AppreciatedBy);
            Assert.Equal(0, blogPost.ViewCount);
            Assert.False(blogPost.IsPublished);
            
            _output.WriteLine("BlogPost default initialization successful");
        }

        [Fact]
        public void BlogDocument_DefaultConstructor_ShouldInitializeCorrectly()
        {
            _output.WriteLine("Testing BlogDocument default initialization...");

            // Act
            var blogDocument = new BlogDocument();

            // Assert
            _output.WriteLine($"BlogDocument initialized with ID: {blogDocument.id}");
            Assert.Equal("blog", blogDocument.id);
            Assert.Equal("aikido-org", blogDocument.tenantid);
            Assert.NotNull(blogDocument.BlogPosts);
            Assert.Empty(blogDocument.BlogPosts);
            
            _output.WriteLine("BlogDocument default initialization successful");
        }
    }
}