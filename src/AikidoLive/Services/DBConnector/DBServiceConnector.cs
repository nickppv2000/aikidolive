using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using AikidoLive.DataModels;

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

        public async Task<bool> UpdatePlaylists(PlaylistsDocument playlistsDocument)
        {
            try
            {
                string databaseName = _databasesDictionary.Keys.First();
                string containerName = _databasesDictionary.Values.First().First();

                _container = _client.GetContainer(databaseName, containerName);
                await _container.ReplaceItemAsync(playlistsDocument, playlistsDocument.Id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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