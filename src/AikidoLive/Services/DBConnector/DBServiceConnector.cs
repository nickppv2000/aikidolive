using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using AikidoLive.DataModels;

namespace AikidoLive.Services.DBConnector
{
    public class DBServiceConnector : IDBServiceConnector, IDisposable
    {
        private readonly CosmosClient _client;
        private Container? _container;
        private List<string> _databases;
        private Dictionary<string, List<string>> _databasesDictionary;

        private string _libraryDbName;
        private string _usersDBName;
        private string _playlistsDBName;
        private string _blogDBName;

        public DBServiceConnector(IConfiguration configuration, CosmosClient client)
        {
            _client = client;
            
            _databasesDictionary = new Dictionary<string, List<string>>();
            _databases = new List<string>();
            var libraryDbSettings = configuration.GetSection("libraryDB");
            _libraryDbName = libraryDbSettings["document"] ?? "";
            var usersDbSettings = configuration.GetSection("usersDB");
            _usersDBName = usersDbSettings["document"] ?? "";

            var playlistsDbSettings = configuration.GetSection("playlistsDB");
            _playlistsDBName = playlistsDbSettings["document"] ?? "";

            var blogDbSettings = configuration.GetSection("blogDB");
            _blogDBName = blogDbSettings["document"] ?? "";

            Console.WriteLine($"DBServiceConnector initializing with:");
            Console.WriteLine($"  - Library document: {_libraryDbName}");
            Console.WriteLine($"  - Users document: {_usersDBName}");
            Console.WriteLine($"  - Playlists document: {_playlistsDBName}");
            Console.WriteLine($"  - Blog document: {_blogDBName}");

            try
            {
                _databases = GetDatabasesListAsync().GetAwaiter().GetResult();
                Console.WriteLine($"Database discovery completed successfully. Found {_databases.Count} databases.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database discovery failed during constructor: {ex.Message}");
                // Don't throw here - let the methods handle the empty dictionary gracefully
            }
        }

        public void Dispose()
        {

        }

        public static async Task<DBServiceConnector> CreateAsync(IConfiguration configuration, CosmosClient client)
        {
            var connector = new DBServiceConnector(configuration, client);
            connector._databases = await connector.GetDatabasesListAsync();
            return connector;
        }

        public List<string> GetDatabasesList()
        {
            return _databases;
        }

        public async Task<List<LibraryDocument>> GetLibraryTitles()
        {
            string databaseName = _databasesDictionary.Keys.First();
            string containerName = _databasesDictionary.Values.First().First();

            _container = _client.GetContainer(databaseName, containerName);

            //var query = new QueryDefinition("SELECT * FROM c");
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id=\"" + _libraryDbName + "\"");
            var iterator = _container.GetItemQueryIterator<LibraryDocument>(query);

            var libraryDocuments = new List<LibraryDocument>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                libraryDocuments.AddRange(response);
            }

            return libraryDocuments;
        }

        public async Task<List<UserList>> GetUsers()
        {
            string databaseName = _databasesDictionary.Keys.First();
            string containerName = _databasesDictionary.Values.First().First();

            _container = _client.GetContainer(databaseName, containerName);

            //var query = new QueryDefinition("SELECT * FROM c");
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id=\"" + _usersDBName + "\"");
            var iterator = _container.GetItemQueryIterator<UserList>(query);

            var usersDocument = new List<UserList>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                usersDocument.AddRange(response);
            }

            return usersDocument;
        }

        public async Task<bool> UpdateUser(UserList userList)
        {
            try
            {
                string databaseName = _databasesDictionary.Keys.First();
                string containerName = _databasesDictionary.Values.First().First();

                _container = _client.GetContainer(databaseName, containerName);
                await _container.ReplaceItemAsync(userList, userList.id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

         public async Task<List<PlaylistsDocument>> GetPlaylists()
        {
            string databaseName = _databasesDictionary.Keys.First();
            string containerName = _databasesDictionary.Values.First().First();

            _container = _client.GetContainer(databaseName, containerName);

            //var query = new QueryDefinition("SELECT * FROM c");
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id=\"" + _playlistsDBName + "\"");
            var iterator = _container.GetItemQueryIterator<PlaylistsDocument>(query);

            var playlistsDocuments = new List<PlaylistsDocument>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                playlistsDocuments.AddRange(response);
            }

            return playlistsDocuments;
        }



        public async Task<List<string>> GetDatabasesListAsync()
        {
            try
            {
                var iterator = _client.GetDatabaseQueryIterator<DatabaseProperties>();
                var databases = new List<string>();

                while (iterator.HasMoreResults)
                {
                    foreach (var database in await iterator.ReadNextAsync())
                    {
                        databases.Add(database.Id);
                        var containers = await GetContainersListAsync(database.Id);
                        _databasesDictionary.Add(database.Id, containers);
                    }
                }

                Console.WriteLine($"Database discovery found {databases.Count} databases:");
                foreach (var db in databases)
                {
                    Console.WriteLine($"  - Database: {db}");
                    if (_databasesDictionary.ContainsKey(db))
                    {
                        foreach (var container in _databasesDictionary[db])
                        {
                            Console.WriteLine($"    - Container: {container}");
                        }
                    }
                }

                return databases;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database discovery failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Failed to discover databases: {ex.Message}", ex);
            }
        }
        public async Task<List<string>> GetContainersListAsync(string databaseName)
        {
            var database = _client.GetDatabase(databaseName);
            var iterator = database.GetContainerQueryIterator<ContainerProperties>();
            var containers = new List<string>();

            while (iterator.HasMoreResults)
            {
                foreach (var container in await iterator.ReadNextAsync())
                {
                    containers.Add(container.Id);
                }
            }

            return containers;
        }

        // Blog-related methods
        public async Task<List<BlogDocument>> GetBlogPosts()
        {
            try
            {
                // Check if database discovery was successful
                if (_databasesDictionary == null || !_databasesDictionary.Any())
                {
                    throw new InvalidOperationException("Database discovery failed. No databases found in _databasesDictionary.");
                }

                string databaseName = _databasesDictionary.Keys.First();
                string containerName = _databasesDictionary.Values.First().First();

                _container = _client.GetContainer(databaseName, containerName);
                
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", _blogDBName);
                
                var iterator = _container.GetItemQueryIterator<BlogDocument>(query);
                var blogDocuments = new List<BlogDocument>();

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    blogDocuments.AddRange(response.ToList());
                }

                return blogDocuments;
            }
            catch (Exception ex)
            {
                // Log the actual error for debugging
                Console.WriteLine($"GetBlogPosts failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                try
                {
                    // If no blog document exists, try to create one
                    var newBlogDocument = new BlogDocument();
                    var createResult = await CreateBlogDocument(newBlogDocument);
                    if (createResult)
                    {
                        return new List<BlogDocument> { newBlogDocument };
                    }
                    else
                    {
                        throw new Exception($"Failed to create new blog document. Original error: {ex.Message}");
                    }
                }
                catch (Exception createEx)
                {
                    throw new Exception($"Failed to get or create blog document. Original error: {ex.Message}, Create error: {createEx.Message}");
                }
            }
        }

        public async Task<bool> CreateBlogDocument(BlogDocument blogDocument)
        {
            try
            {
                // Check if database discovery was successful
                if (_databasesDictionary == null || !_databasesDictionary.Any())
                {
                    throw new InvalidOperationException("Database discovery failed. No databases found in _databasesDictionary.");
                }

                string databaseName = _databasesDictionary.Keys.First();
                string containerName = _databasesDictionary.Values.First().First();

                _container = _client.GetContainer(databaseName, containerName);
                await _container.CreateItemAsync(blogDocument);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateBlogDocument failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateBlogDocument(BlogDocument blogDocument)
        {
            try
            {
                // Check if database discovery was successful
                if (_databasesDictionary == null || !_databasesDictionary.Any())
                {
                    throw new InvalidOperationException("Database discovery failed. No databases found in _databasesDictionary.");
                }

                string databaseName = _databasesDictionary.Keys.First();
                string containerName = _databasesDictionary.Values.First().First();

                _container = _client.GetContainer(databaseName, containerName);
                await _container.ReplaceItemAsync(blogDocument, blogDocument.id);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateBlogDocument failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}