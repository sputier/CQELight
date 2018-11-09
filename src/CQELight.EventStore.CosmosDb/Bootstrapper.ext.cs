using CQELight.EventStore.CosmosDb.Service;
using CQELight.EventStore.CosmosDb.Common;
using System;
using CQELight.EventStore.CosmosDb;

namespace CQELight
{
    public static class BootstrapperExtensions
    {
        /// <summary>
        /// Use CosmosDB DocumentDB with a one or multiple server urls. 
        /// Multiples urls are usefull when a replica set has been created.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="endPointUrl">Url of CosmosDB host server.</param>
        /// <param name="primaryKey">DataBase primary key</param>
        /// <returns>Bootstrapper instance.</returns>
        public static Bootstrapper UseCosmosDbAsEventStore(this Bootstrapper bootstrapper, string endPointUrl, string primaryKey)
        {
            if (string.IsNullOrEmpty(endPointUrl))
                throw new ArgumentNullException("BootstrapperExtensions.UseCosmosDbAsEventStore : endPointUrl have to be definied to use CosmosDb Event Store.", nameof(endPointUrl));

            if (string.IsNullOrEmpty(primaryKey))
                throw new ArgumentNullException("BootstrapperExtensions.UseCosmosDbAsEventStore : primarykey have to be definied to use CosmosDb Event Store.", nameof(primaryKey));

            var service = new CosmosDbEventStoreBootstrappService
            {
                BootstrappAction = (ctx) =>
                {
                    EventStoreAzureDbContext.Activate(new AzureDbConfiguration(endPointUrl, primaryKey));
                    EventStoreManager.Activate();
                }
            };
            bootstrapper.AddService(service);
            return bootstrapper;            
        }
    }
}
