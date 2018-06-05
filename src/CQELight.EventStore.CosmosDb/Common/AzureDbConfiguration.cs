namespace CQELight.EventStore.CosmosDb.Common
{
    internal class AzureDbConfiguration
    {

        #region Properties

        public string EndPointUrl { get;private set; }
        public string PrimaryKey { get; private set; }

        #endregion

        #region Ctor

        internal AzureDbConfiguration(string endPointUrl,string primaryKey)
        {
            EndPointUrl = endPointUrl;
            PrimaryKey = primaryKey;
        }

        #endregion

    }
}
