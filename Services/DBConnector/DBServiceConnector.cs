using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using AikidoLive.DataModels;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace AikidoLive.Services.DBConnector
{
    public class DBServiceConnector : IDisposable
    {
        private readonly CosmosClient _client;
        private Container? _container;
        private List<string> _databases;
        private Dictionary<string, List<string>> _databasesDictionary;

        private string _libraryDbName;
        private string _usersDBName;
        private string _playlistsDBName;

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

            _databases = GetDatabasesListAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {

        }

        public CosmosClient getDbClientContext()
        {
            return _client;
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

        public async Task<Task<IdentityResult>> CreateUser(IdentityUser user)
        {
            string databaseName = _databasesDictionary.Keys.First();
            string containerName = _databasesDictionary.Values.First().First();

            _container = _client.GetContainer(databaseName, containerName);

            var query = new QueryDefinition("SELECT * FROM c WHERE c.id=\"" + _usersDBName + "\"");
            //var iterator = _container.GetItemQueryIterator<UserList>(query);

            /*var usersDocument = new List<UserList>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                if (response.Count == 0)
                {
                    var usrList = new UserList(_usersDBName, new List<AikidoLive.DataModels.User>());
                    usersDocument.Add(usrList);
                    break;
                }
                else
                {
                    usersDocument.AddRange(response);
                }
            }

            var userList = usersDocument.First();*/
            //var userList = new UserList(_usersDBName, new List<AikidoLive.DataModels.User>());

            //userList.Users.Add(new AikidoLive.DataModels.User(user));

            var usr = new AikidoLive.DataModels.User(user);

            try
            {
                Console.WriteLine($"Upserting item: {JsonConvert.SerializeObject(usr)}");
                Console.WriteLine($"Partition key: {user.Id}");


                ItemResponse<AikidoLive.DataModels.User> createUserResponse = await _container.UpsertItemAsync(usr, new PartitionKey(user.Id));
                return Task.FromResult(IdentityResult.Success);
            }
            catch (CosmosException ex) // CosmosDB-specific exception
            {
                Console.WriteLine(ex.Message);
                // Log the error or do something with it
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = $"Could not create user: {ex.Message}" }));
            }
            catch (Exception ex) // Anything else
            {
                // Log the error or do something with it
                Console.WriteLine(ex.Message);
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = $"Could not create user: {ex.Message}" }));
            }

            return Task.FromResult(IdentityResult.Success);
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
            var iterator = _client.GetDatabaseQueryIterator<DatabaseProperties>();
            var databases = new List<string>();

            while (iterator.HasMoreResults)
            {
                foreach (var database in await iterator.ReadNextAsync())
                {
                    databases.Add(database.Id);
                    var containers = GetContainersListAsync(database.Id).Result;
                    _databasesDictionary.Add(database.Id, containers);
                }
            }

            return databases;
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
    }

}