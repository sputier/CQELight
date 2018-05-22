using Microsoft.Azure.Documents.Client;
using System;


namespace CQELight.EventStore.CosmosDb.Common
{
    internal class EventStoreAzureDbContext
    {
        private readonly AzureDbConfiguration _configuration;

        public DocumentClient client { get; private set; }

        public EventStoreAzureDbContext(AzureDbConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
    }
}
