using CQELight.Dispatcher;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;

namespace CQELight.EventStore.CosmosDb.Common
{
    internal static class EventStoreAzureDbContext
    {

        #region Consts

        public const string CONST_DB_NAME = "CQELight_Events";
        public const string CONST_COLLECTION_NAME = "events";
        public const string CONST_ID_FIELD = "_id";

        #endregion

        #region Properties

        internal static AzureDbConfiguration Configuration { get; set; }
        internal static DocumentClient Client { get; private set; }
        internal static Uri DatabaseLink { get; set; }

        #endregion

        #region Ctor

        public static void Activate(AzureDbConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            Client = new DocumentClient(new Uri(Configuration.EndPointUrl), Configuration.PrimaryKey);
            InitDocumentDb().GetAwaiter().GetResult();

            CoreDispatcher.OnEventDispatched += (e) => new CosmosDbEventStore().StoreDomainEventAsync(e);
        }

        #endregion

        #region Private methods

        private static async Task InitDocumentDb()
        {
            await Client.CreateDatabaseIfNotExistsAsync(new Database { Id = CONST_DB_NAME }).ConfigureAwait(false);
            await Client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(CONST_DB_NAME), new DocumentCollection { Id = CONST_COLLECTION_NAME }).ConfigureAwait(false);
            DatabaseLink = UriFactory.CreateDocumentCollectionUri(CONST_DB_NAME, CONST_COLLECTION_NAME);
        }

        #endregion
    }
}
