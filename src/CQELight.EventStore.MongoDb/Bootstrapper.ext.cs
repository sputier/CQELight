using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.MongoDb;
using CQELight.EventStore.MongoDb.Common;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight
{
    public static class BootstrapperExtensions
    {
        /// <summary>
        /// Use MongoDb with a one or multiple server urls. 
        /// Multiples urls are usefull when a replica set has been created.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="serversUrl">Collection of servers to use MongoDb on it.</param>
        /// <returns>Bootstrapper instance.</returns>
        public static Bootstrapper UseMongoDbAsEventStore(this Bootstrapper bootstrapper, params string[] serversUrl)
        {
            if (serversUrl == null)
            {
                throw new ArgumentNullException(nameof(serversUrl));
            }
            if (serversUrl.Length == 0)
            {
                throw new ArgumentException("Bootstrapper.UseMongoDbAsEventStore() : At least one url should be provided, for main server.", nameof(serversUrl));
            }
            if (serversUrl.Any(u => !u.StartsWith("mongodb://", StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new ArgumentException("Bootstrapper.UseMongoDbAsEventStore() : All provided url should be formatted like 'mongodb://{ipAdress[:port]}'", nameof(serversUrl));
            }
            var service = new MongoDbEventStoreBootstrappService
            {
                BootstrappAction = () =>
                {
                    BsonSerializer.RegisterSerializer(typeof(Type), new TypeSerializer());
                    BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer());
                    EventStoreManager.ServersUrls = string.Join(",", serversUrl);
                    EventStoreManager.Activate();
                }
            };
            bootstrapper.AddService(service);
            return bootstrapper;
        }

        private static void AddBehaviorToManager(ISnapshotBehavior behavior, params Type[] eventTypes)
        {
            foreach (var item in eventTypes)
            {
                EventStoreManager.Behaviors.Add(item, behavior);
            }
        }
    }
}
