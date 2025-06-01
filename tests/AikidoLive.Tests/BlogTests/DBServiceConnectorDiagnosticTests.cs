using AikidoLive.Services.DBConnector;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AikidoLive.Tests.BlogTests
{
    /// <summary>
    /// Tests to diagnose DBServiceConnector database discovery and connection issues
    /// </summary>
    public class DBServiceConnectorDiagnosticTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;
        private readonly CosmosClient _cosmosClient;

        public DBServiceConnectorDiagnosticTests(ITestOutputHelper output)
        {
            _output = output;

            // Load test configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false);
            
            _configuration = builder.Build();

            // Create Cosmos client
            var cosmosDbConfig = _configuration.GetSection("CosmosDb");
            _cosmosClient = new CosmosClient(
                cosmosDbConfig["Account"], 
                cosmosDbConfig["Key"]
            );
        }

        [Fact]
        public async Task Test_CosmosDB_DirectAccess_ShouldWork()
        {
            _output.WriteLine("Testing direct Cosmos DB access...");

            try
            {
                var cosmosDbConfig = _configuration.GetSection("CosmosDb");
                var databaseName = cosmosDbConfig["DatabaseName"];
                
                _output.WriteLine($"Database name from config: {databaseName}");
                
                var database = _cosmosClient.GetDatabase(databaseName);
                var response = await database.ReadAsync();
                
                _output.WriteLine($"Database read successful. Status: {response.StatusCode}");
                _output.WriteLine($"Database ID: {response.Resource.Id}");
                
                // List containers
                var containerIterator = database.GetContainerQueryIterator<dynamic>();
                _output.WriteLine("Available containers:");
                
                while (containerIterator.HasMoreResults)
                {
                    var containers = await containerIterator.ReadNextAsync();
                    foreach (var container in containers)
                    {
                        _output.WriteLine($"  - Container: {container.id}");
                    }
                }
                
                Assert.True(true, "Direct database access successful");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Direct database access failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task Test_DBServiceConnector_Initialization_ShouldWork()
        {
            _output.WriteLine("Testing DBServiceConnector initialization...");

            try
            {
                var dbConnector = new DBServiceConnector(_configuration, _cosmosClient);
                
                _output.WriteLine("DBServiceConnector created successfully");
                
                // Test the database discovery by calling a method that would use it
                var blogPosts = await dbConnector.GetBlogPosts();
                
                _output.WriteLine($"GetBlogPosts returned: {blogPosts?.Count ?? 0} documents");
                
                Assert.NotNull(blogPosts);
                
                dbConnector.Dispose();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"DBServiceConnector initialization failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task Test_DBServiceConnector_DatabasesList_ShouldNotBeEmpty()
        {
            _output.WriteLine("Testing DBServiceConnector database discovery...");

            try
            {
                // We need to access the private fields to diagnose the issue
                // Let's create a DBServiceConnector and inspect its behavior
                var dbConnector = new DBServiceConnector(_configuration, _cosmosClient);
                
                // Try to call GetBlogPosts which should reveal if databases are discovered correctly
                var blogPosts = await dbConnector.GetBlogPosts();
                
                _output.WriteLine($"GetBlogPosts call completed. Result count: {blogPosts?.Count ?? 0}");
                
                // If this doesn't throw an exception, the database discovery is working
                Assert.NotNull(blogPosts);
                
                dbConnector.Dispose();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Database discovery test failed: {ex.Message}");
                _output.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Let's try to understand what's happening by examining the cosmos client directly
                await DiagnoseCosmosClient();
                
                throw;
            }
        }

        private async Task DiagnoseCosmosClient()
        {
            _output.WriteLine("Diagnosing Cosmos Client...");
            
            try
            {
                var cosmosDbConfig = _configuration.GetSection("CosmosDb");
                var account = cosmosDbConfig["Account"];
                var key = cosmosDbConfig["Key"];
                
                _output.WriteLine($"Account URI: {account}");
                _output.WriteLine($"Key length: {key?.Length ?? 0}");
                
                // List all databases
                var databaseIterator = _cosmosClient.GetDatabaseQueryIterator<dynamic>();
                _output.WriteLine("Available databases:");
                
                while (databaseIterator.HasMoreResults)
                {
                    var databases = await databaseIterator.ReadNextAsync();
                    foreach (var database in databases)
                    {
                        _output.WriteLine($"  - Database: {database.id}");
                        
                        // For each database, list containers
                        try
                        {
                            var db = _cosmosClient.GetDatabase(database.id);
                            var containerIterator = db.GetContainerQueryIterator<dynamic>();
                            
                            while (containerIterator.HasMoreResults)
                            {
                                var containers = await containerIterator.ReadNextAsync();
                                foreach (var container in containers)
                                {
                                    _output.WriteLine($"    - Container: {container.id}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _output.WriteLine($"    - Error listing containers: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Cosmos client diagnosis failed: {ex.Message}");
            }
        }

        [Fact]
        public async Task Test_BlogDocument_Creation_WithSpecificContainer()
        {
            _output.WriteLine("Testing blog document creation with specific container...");

            try
            {
                var cosmosDbConfig = _configuration.GetSection("CosmosDb");
                var databaseName = cosmosDbConfig["DatabaseName"];
                
                // Let's try to access the most likely container name
                var database = _cosmosClient.GetDatabase(databaseName);
                
                // Common container names to try
                string[] containerNames = { "items", "documents", "data", "container1", "default" };
                
                foreach (var containerName in containerNames)
                {
                    try
                    {
                        _output.WriteLine($"Trying container: {containerName}");
                        
                        var container = database.GetContainer(containerName);
                        var response = await container.ReadContainerAsync();
                        
                        _output.WriteLine($"Successfully accessed container: {containerName}");
                        _output.WriteLine($"Container partition key path: {response.Resource.PartitionKeyPath}");
                        
                        // Try to query for existing blog documents
                        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = 'blog'");
                        var iterator = container.GetItemQueryIterator<dynamic>(query);
                        
                        while (iterator.HasMoreResults)
                        {
                            var results = await iterator.ReadNextAsync();
                            _output.WriteLine($"Found {results.Count} existing blog documents in {containerName}");
                        }
                        
                        break; // If we get here, we found a working container
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Container {containerName} failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Blog document creation test failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public void Dispose()
        {
            _cosmosClient?.Dispose();
        }
    }
}