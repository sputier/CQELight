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
        public const string CONST_EVENTS_COLLECTION_NAME = "events";
        public const string CONST_SNAPSHOT_COLLECTION_NAME = "snapshots";
        public const string CONST_ID_FIELD = "_id";

        #endregion

        #region Properties

        internal static AzureDbConfiguration Configuration { get; set; }
        internal static DocumentClient Client { get; private set; }
        internal static Uri EventsDatabaseLink { get; set; }
        internal static Uri SnapshotDatabaseLink { get; set; }

        #endregion

        #region Ctor

        public static async Task Activate(AzureDbConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Client = new DocumentClient(new Uri(Configuration.EndPointUrl), Configuration.PrimaryKey);

            await InitDocumentDbAsync().ConfigureAwait(false);

            CoreDispatcher.OnEventDispatched += EventStoreManager.OnEventDispatchedMethod;
        }

        #endregion

        #region Internal methods

        internal static async Task InitDocumentDbAsync()
        {
            await Client.CreateDatabaseIfNotExistsAsync(new Database { Id = CONST_DB_NAME }).ConfigureAwait(false);
            await Client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(CONST_DB_NAME), 
                new DocumentCollection { Id = CONST_EVENTS_COLLECTION_NAME }).ConfigureAwait(false);
            await Client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(CONST_DB_NAME),
                new DocumentCollection { Id = CONST_SNAPSHOT_COLLECTION_NAME }).ConfigureAwait(false);
            EventsDatabaseLink = UriFactory.CreateDocumentCollectionUri(CONST_DB_NAME, CONST_EVENTS_COLLECTION_NAME);
            SnapshotDatabaseLink = UriFactory.CreateDocumentCollectionUri(CONST_DB_NAME, CONST_SNAPSHOT_COLLECTION_NAME);
        }

        #endregion
    }
}
