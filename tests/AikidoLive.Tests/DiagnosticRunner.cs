using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using AikidoLive.Services.DBConnector;

namespace AikidoLive.Tests
{
    /// <summary>
    /// Simple test runner to quickly diagnose blog post saving issues
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Blog Post Saving Diagnostic Tool ===");
            Console.WriteLine();
            
            try
            {
                // Load configuration
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Test.json", optional: false);
                
                var configuration = builder.Build();
                
                // Test 1: Basic configuration validation
                Console.WriteLine("1. Testing configuration...");
                var cosmosDbConfig = configuration.GetSection("CosmosDb");
                var account = cosmosDbConfig["Account"];
                var key = cosmosDbConfig["Key"];
                var databaseName = cosmosDbConfig["DatabaseName"];
                var blogDocument = configuration["blogDB:document"];
                
                Console.WriteLine($"   Account: {account}");
                Console.WriteLine($"   Database: {databaseName}");
                Console.WriteLine($"   Blog Document: {blogDocument}");
                Console.WriteLine($"   Key present: {!string.IsNullOrEmpty(key)}");
                Console.WriteLine("   ✓ Configuration loaded");
                Console.WriteLine();
                
                // Test 2: Cosmos Client connection
                Console.WriteLine("2. Testing Cosmos Client connection...");
                using var cosmosClient = new CosmosClient(account, key);
                var database = cosmosClient.GetDatabase(databaseName);
                var dbResponse = await database.ReadAsync();
                Console.WriteLine($"   ✓ Connected to database: {dbResponse.Resource.Id}");
                Console.WriteLine();
                
                // Test 3: List containers
                Console.WriteLine("3. Discovering containers...");
                var containerIterator = database.GetContainerQueryIterator<ContainerProperties>();
                var containerFound = false;
                string firstContainerName = null;
                
                while (containerIterator.HasMoreResults)
                {
                    var containers = await containerIterator.ReadNextAsync();
                    foreach (var container in containers)
                    {
                        Console.WriteLine($"   Found container: {container.Id}");
                        if (firstContainerName == null)
                        {
                            firstContainerName = container.Id;
                        }
                        containerFound = true;
                    }
                }
                
                if (!containerFound)
                {
                    Console.WriteLine("   ❌ No containers found in database!");
                    return;
                }
                
                Console.WriteLine($"   ✓ Found containers, will use: {firstContainerName}");
                Console.WriteLine();
                
                // Test 4: DBServiceConnector initialization
                Console.WriteLine("4. Testing DBServiceConnector initialization...");
                try
                {
                    var dbConnector = new DBServiceConnector(configuration, cosmosClient);
                    Console.WriteLine("   ✓ DBServiceConnector created successfully");
                    
                    // Test database discovery
                    var databases = dbConnector.GetDatabasesList();
                    Console.WriteLine($"   Discovered {databases.Count} databases:");
                    foreach (var db in databases)
                    {
                        Console.WriteLine($"     - {db}");
                    }
                    
                    dbConnector.Dispose();
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ DBServiceConnector initialization failed: {ex.Message}");
                    Console.WriteLine($"   Stack trace: {ex.StackTrace}");
                    return;
                }
                
                // Test 5: Blog document operations
                Console.WriteLine("5. Testing blog document operations...");
                try
                {
                    var dbConnector = new DBServiceConnector(configuration, cosmosClient);
                    
                    // Try to get blog posts
                    Console.WriteLine("   Attempting to get blog posts...");
                    var blogPosts = await dbConnector.GetBlogPosts();
                    Console.WriteLine($"   ✓ GetBlogPosts returned {blogPosts?.Count ?? 0} documents");
                    
                    if (blogPosts != null && blogPosts.Count > 0)
                    {
                        var blogDoc = blogPosts[0];
                        Console.WriteLine($"   Blog document ID: {blogDoc.id}");
                        Console.WriteLine($"   Blog posts count: {blogDoc.BlogPosts?.Count ?? 0}");
                    }
                    
                    dbConnector.Dispose();
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ Blog document operations failed: {ex.Message}");
                    Console.WriteLine($"   Stack trace: {ex.StackTrace}");
                    return;
                }
                
                Console.WriteLine("=== All tests passed! Blog saving should work. ===");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Diagnostic failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}