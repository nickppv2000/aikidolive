using AikidoLive.DataModels;
using AikidoLive.Services.Blog;
using AikidoLive.Services.DBConnector;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AikidoLive.Tests.BlogTests
{
    /// <summary>
    /// Integration tests to identify issues with blog post saving functionality.
    /// These tests interact with the actual Cosmos DB to identify where the problem occurs.
    /// </summary>
    public class BlogIntegrationTests : IAsyncDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;
        private readonly CosmosClient _cosmosClient;
        private readonly DBServiceConnector _dbConnector;
        private readonly BlogService _blogService;
        private readonly string _testDocumentId;

        public BlogIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            _testDocumentId = $"blog-test-{Guid.NewGuid():N}";

            // Load test configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false);
            
            _configuration = builder.Build();

            // Override the blog document ID for testing
            _configuration["blogDB:document"] = _testDocumentId;

            // Create Cosmos client
            var cosmosDbConfig = _configuration.GetSection("CosmosDb");
            _cosmosClient = new CosmosClient(
                cosmosDbConfig["Account"], 
                cosmosDbConfig["Key"]
            );

            // Create DB connector and blog service
            _dbConnector = new DBServiceConnector(_configuration, _cosmosClient);
            _blogService = new BlogService(_dbConnector);

            _output.WriteLine($"Test initialized with document ID: {_testDocumentId}");
        }

        [Fact]
        public async Task Test_DatabaseConnection_ShouldConnect()
        {
            _output.WriteLine("Testing database connection...");

            try
            {
                // Test if we can connect to the database
                var cosmosDbConfig = _configuration.GetSection("CosmosDb");
                var databaseName = cosmosDbConfig["DatabaseName"];
                
                _output.WriteLine($"Connecting to database: {databaseName}");
                
                var database = _cosmosClient.GetDatabase(databaseName);
                var response = await database.ReadAsync();
                
                _output.WriteLine($"Database connection successful. Status: {response.StatusCode}");
                Assert.NotNull(response);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Database connection failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task Test_CreateBlogDocument_ShouldSucceed()
        {
            _output.WriteLine("Testing blog document creation...");

            try
            {
                var newBlogDocument = new BlogDocument
                {
                    id = _testDocumentId,
                    tenantid = "aikido-org-test"
                };

                _output.WriteLine($"Creating blog document with ID: {_testDocumentId}");
                
                var result = await _dbConnector.CreateBlogDocument(newBlogDocument);
                
                _output.WriteLine($"Blog document creation result: {result}");
                Assert.True(result, "Failed to create blog document");
                
                // Verify the document was created
                var retrievedDocuments = await _dbConnector.GetBlogPosts();
                _output.WriteLine($"Retrieved {retrievedDocuments?.Count ?? 0} blog documents");
                
                Assert.NotNull(retrievedDocuments);
                Assert.NotEmpty(retrievedDocuments);
                
                var createdDoc = retrievedDocuments.Find(d => d.id == _testDocumentId);
                Assert.NotNull(createdDoc);
                _output.WriteLine($"Successfully verified blog document creation with ID: {createdDoc.id}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Blog document creation failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task Test_GetBlogPosts_WhenNoDocumentExists_ShouldCreateOne()
        {
            _output.WriteLine("Testing GetBlogPosts when no document exists...");

            try
            {
                // This should create a new document if none exists
                var blogDocuments = await _dbConnector.GetBlogPosts();
                
                _output.WriteLine($"GetBlogPosts returned {blogDocuments?.Count ?? 0} documents");
                
                Assert.NotNull(blogDocuments);
                Assert.NotEmpty(blogDocuments);
                
                var document = blogDocuments[0];
                Assert.NotNull(document);
                Assert.NotNull(document.BlogPosts);
                
                _output.WriteLine($"Blog document created with ID: {document.id}");
                _output.WriteLine($"Blog posts count: {document.BlogPosts.Count}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"GetBlogPosts failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task Test_CreateBlogPost_EndToEnd_ShouldSucceed()
        {
            _output.WriteLine("Testing end-to-end blog post creation...");

            try
            {
                var testPost = new BlogPost
                {
                    Title = "Integration Test Post",
                    Content = "This is a test post created by integration tests.",
                    AuthorEmail = "test@example.com",
                    AuthorName = "Test Author",
                    IsPublished = true,
                    Tags = new List<string> { "test", "integration", "aikido" }
                };

                _output.WriteLine($"Creating blog post: {testPost.Title}");
                _output.WriteLine($"Post ID: {testPost.Id}");
                
                var result = await _blogService.CreateBlogPostAsync(testPost);
                
                _output.WriteLine($"Blog post creation result: {result}");
                Assert.True(result, "Failed to create blog post through BlogService");
                
                // Verify the post was created by retrieving it
                var retrievedPost = await _blogService.GetBlogPostByIdAsync(testPost.Id);
                
                _output.WriteLine($"Retrieved post: {retrievedPost?.Title ?? "null"}");
                Assert.NotNull(retrievedPost);
                Assert.Equal(testPost.Title, retrievedPost.Title);
                Assert.Equal(testPost.Content, retrievedPost.Content);
                Assert.Equal(testPost.AuthorEmail, retrievedPost.AuthorEmail);
                
                _output.WriteLine("Blog post creation and retrieval successful!");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"End-to-end blog post creation failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task Test_UpdateBlogPost_ShouldSucceed()
        {
            _output.WriteLine("Testing blog post update...");

            try
            {
                // First create a post
                var originalPost = new BlogPost
                {
                    Title = "Original Title",
                    Content = "Original content",
                    AuthorEmail = "test@example.com",
                    AuthorName = "Test Author",
                    IsPublished = false
                };

                _output.WriteLine($"Creating original post: {originalPost.Id}");
                var createResult = await _blogService.CreateBlogPostAsync(originalPost);
                Assert.True(createResult, "Failed to create original post");
                
                // Now update the post
                originalPost.Title = "Updated Title";
                originalPost.Content = "Updated content";
                originalPost.IsPublished = true;
                originalPost.UpdatedAt = DateTime.UtcNow;

                _output.WriteLine($"Updating post: {originalPost.Id}");
                var updateResult = await _blogService.UpdateBlogPostAsync(originalPost);
                
                _output.WriteLine($"Blog post update result: {updateResult}");
                Assert.True(updateResult, "Failed to update blog post");
                
                // Verify the update
                var retrievedPost = await _blogService.GetBlogPostByIdAsync(originalPost.Id);
                Assert.NotNull(retrievedPost);
                Assert.Equal("Updated Title", retrievedPost.Title);
                Assert.Equal("Updated content", retrievedPost.Content);
                Assert.True(retrievedPost.IsPublished);
                
                _output.WriteLine("Blog post update successful!");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Blog post update failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task Test_BlogService_GetPublishedPosts_ShouldFilterCorrectly()
        {
            _output.WriteLine("Testing published posts filtering...");

            try
            {
                // Create a published post
                var publishedPost = new BlogPost
                {
                    Title = "Published Post",
                    Content = "This post is published",
                    AuthorEmail = "test@example.com",
                    AuthorName = "Test Author",
                    IsPublished = true
                };

                // Create a draft post
                var draftPost = new BlogPost
                {
                    Title = "Draft Post",
                    Content = "This post is a draft",
                    AuthorEmail = "test@example.com",
                    AuthorName = "Test Author",
                    IsPublished = false
                };

                _output.WriteLine("Creating published and draft posts...");
                await _blogService.CreateBlogPostAsync(publishedPost);
                await _blogService.CreateBlogPostAsync(draftPost);
                
                // Get published posts
                var publishedPosts = await _blogService.GetPublishedBlogPostsAsync();
                
                _output.WriteLine($"Retrieved {publishedPosts.Count} published posts");
                
                // Should contain at least the published post we created
                Assert.NotEmpty(publishedPosts);
                
                // All posts should be published
                Assert.All(publishedPosts, post => Assert.True(post.IsPublished));
                
                // Should contain our published post
                Assert.Contains(publishedPosts, p => p.Id == publishedPost.Id);
                
                // Should not contain our draft post
                Assert.DoesNotContain(publishedPosts, p => p.Id == draftPost.Id);
                
                _output.WriteLine("Published posts filtering working correctly!");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Published posts filtering test failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task Test_DatabaseSchema_BlogDocument_Structure()
        {
            _output.WriteLine("Testing blog document schema and structure...");

            try
            {
                // Create a blog document with various data types
                var testDocument = new BlogDocument();
                var testPost = new BlogPost
                {
                    Title = "Schema Test Post",
                    Content = "<h1>HTML Content</h1><p>With special characters: üñíçødé & symbols!</p>",
                    AuthorEmail = "schema.test@example.com",
                    AuthorName = "Schema Tester",
                    IsPublished = true,
                    Tags = new List<string> { "schema", "test", "special-chars" },
                    ViewCount = 42
                };

                // Add a comment to test nested structure
                var testComment = new Comment
                {
                    Content = "Test comment with special chars: 测试评论",
                    AuthorEmail = "commenter@example.com",
                    AuthorName = "Test Commenter",
                    CreatedAt = DateTime.UtcNow
                };
                testPost.Comments.Add(testComment);
                
                // Add appreciation
                testPost.AppreciatedBy.Add("appreciator@example.com");
                
                testDocument.BlogPosts.Add(testPost);

                _output.WriteLine("Testing document creation with complex data...");
                
                // Set a specific ID for this test
                var schemaTestId = $"schema-test-{Guid.NewGuid():N}";
                testDocument.id = schemaTestId;
                
                var createResult = await _dbConnector.CreateBlogDocument(testDocument);
                Assert.True(createResult, "Failed to create complex blog document");
                
                // Retrieve and verify all data was preserved
                var documents = await _dbConnector.GetBlogPosts();
                var retrievedDoc = documents.Find(d => d.id == schemaTestId);
                
                Assert.NotNull(retrievedDoc);
                Assert.NotEmpty(retrievedDoc.BlogPosts);
                
                var retrievedPost = retrievedDoc.BlogPosts[0];
                Assert.Equal(testPost.Title, retrievedPost.Title);
                Assert.Equal(testPost.Content, retrievedPost.Content);
                Assert.Equal(testPost.Tags.Count, retrievedPost.Tags.Count);
                Assert.Equal(testPost.Comments.Count, retrievedPost.Comments.Count);
                Assert.Equal(testPost.AppreciatedBy.Count, retrievedPost.AppreciatedBy.Count);
                Assert.Equal(testPost.ViewCount, retrievedPost.ViewCount);
                
                _output.WriteLine("Blog document schema validation successful!");
                
                // Clean up this test document
                await CleanupTestDocument(schemaTestId);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Schema validation failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task CleanupTestDocument(string documentId)
        {
            try
            {
                _output.WriteLine($"Cleaning up test document: {documentId}");
                
                var cosmosDbConfig = _configuration.GetSection("CosmosDb");
                var databaseName = cosmosDbConfig["DatabaseName"];
                var database = _cosmosClient.GetDatabase(databaseName);
                
                // We need to find the container - let's check what containers exist
                var containerIterator = database.GetContainerQueryIterator<dynamic>();
                while (containerIterator.HasMoreResults)
                {
                    var containers = await containerIterator.ReadNextAsync();
                    foreach (var container in containers)
                    {
                        try
                        {
                            var containerObj = database.GetContainer(container.id);
                            await containerObj.DeleteItemAsync<BlogDocument>(
                                documentId, 
                                new PartitionKey(documentId)
                            );
                            _output.WriteLine($"Cleaned up document {documentId} from container {container.id}");
                            return;
                        }
                        catch
                        {
                            // Continue to next container
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Cleanup failed (this is usually OK): {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                _output.WriteLine($"Cleaning up test document: {_testDocumentId}");
                await CleanupTestDocument(_testDocumentId);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Cleanup during disposal failed: {ex.Message}");
            }
            finally
            {
                _dbConnector?.Dispose();
                _cosmosClient?.Dispose();
            }
        }
    }
}