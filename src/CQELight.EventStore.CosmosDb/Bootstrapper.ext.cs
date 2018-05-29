using CQELight.EventStore.CosmosDb.Service;
using CQELight.EventStore.CosmosDb.Common;

namespace CQELight.EventStore.CosmosDb
{
    public static class BootstrapperExtensions
    {
        /// <summary>
        /// Use MongoDb with a one or multiple server urls. 
        /// Multiples urls are usefull when a replica set has been created.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="endPointUrl">Url du serveur hébergeant le moteur CosmosDB.</param>
        /// <param name="primaryKey">primary key de la base</param>
        /// <returns>Bootstrapper instance.</returns>
        public static Bootstrapper UseCosmosDbAsEventStore(this Bootstrapper bootstrapper, string endPointUrl, string primaryKey)
        {
            var service = new CosmosDbEventStoreBootstrappService
            {
                BootstrappAction = () => EventStoreAzureDbContext.Activate(new AzureDbConfiguration(endPointUrl, primaryKey))
            };
            bootstrapper.AddService(service);
            return bootstrapper;            
        }
    }
}
