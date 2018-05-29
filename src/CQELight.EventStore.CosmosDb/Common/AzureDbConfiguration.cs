namespace CQELight.EventStore.CosmosDb.Common
{
    internal class AzureDbConfiguration
    {
        public string EndPointUrl { get;private set; }
        public string PrimaryKey { get; private set; }        

        internal AzureDbConfiguration(string endPointUrl,string primaryKey)
        {
            EndPointUrl = endPointUrl;
            PrimaryKey = primaryKey;
        }
    }
}
