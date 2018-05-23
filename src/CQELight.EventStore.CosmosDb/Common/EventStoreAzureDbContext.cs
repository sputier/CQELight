using Microsoft.Azure.Documents.Client;
using System;


namespace CQELight.EventStore.CosmosDb.Common
{
    internal class EventStoreAzureDbContext
    {
        private readonly AzureDbConfiguration _configuration;

        public DocumentClient Client { get; private set; }
        public const string CONST_DB_NAME = "CQELight_Events";
        public const string CONST_COLLECTION_NAME = "events";
        public const string CONST_ID_FIELD = "_id";

        public EventStoreAzureDbContext(AzureDbConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            Client = new DocumentClient(new Uri(_configuration.EndPointUrl), _configuration.PrimaryKey);
        }
    }
}
