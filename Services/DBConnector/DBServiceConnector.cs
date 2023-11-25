using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using AikidoLive.DataModels;

namespace AikidoLive.Services.DBConnector
{
    public class DBServiceConnector
    {
        private readonly CosmosClient _client;
        private Container? _container;
        private List<string> _databases;
        private Dictionary<string, List<string>> _databasesDictionary;

        public DBServiceConnector(IConfiguration configuration)
        {
            var cosmosDbSettings = configuration.GetSection("CosmosDb");
            _client = new CosmosClient(cosmosDbSettings["Account"], cosmosDbSettings["Key"]);
            _databasesDictionary = new Dictionary<string, List<string>>();
            _databases = new List<string>();
            
        }

        public static async Task<DBServiceConnector> CreateAsync(IConfiguration configuration)
        {
            var connector = new DBServiceConnector(configuration);
            connector._databases = await connector.GetDatabasesListAsync();
            return connector;
        }

        public List<string> GetDatabasesList()
        {
            return _databases;
        }

        public async Task<List<LibraryDocument>> GetLibraryTitles()
        {
            _container = _client.GetContainer(_databasesDictionary.Keys.First(), _databasesDictionary.Values.First().First());

            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = _container.GetItemQueryIterator<LibraryDocument>(query);

            var libraryDocuments = new List<LibraryDocument>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                libraryDocuments.AddRange(response);
            }

            return libraryDocuments;
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