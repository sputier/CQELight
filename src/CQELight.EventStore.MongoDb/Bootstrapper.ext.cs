using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.MongoDb;
using CQELight.EventStore.MongoDb.Common;
using CQELight.IoC;
using CQELight.Tools.Extensions;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight
{
    public static class BootstrapperExtensions
    {

        #region Public static methods

        /// <summary>
        /// Use MongoDb with a one or multiple server urls. 
        /// Multiples urls are usefull when a replica set has been created.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="options">Options to bootstrap MongoDb as Event Store.</param>
        /// <returns>Bootstrapper instance.</returns>
        public static Bootstrapper UseMongoDbAsEventStore(this Bootstrapper bootstrapper, MongoDbEventStoreBootstrapperConfiguration options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var service = new MongoDbEventStoreBootstrappService
            {
                BootstrappAction = (ctx) =>
                {
                    BsonSerializer.RegisterSerializer(typeof(Type), new TypeSerializer());
                    BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer());
                    BsonSerializer.RegisterSerializer(typeof(object), new ObjectSerializer());
                    EventStoreManager.ServersUrls = string.Join(",", options.ServerUrls);
                    if (options.SnapshotBehaviorProvider != null)
                    {
                        if (ctx.IsServiceRegistered(BootstrapperServiceType.IoC))
                        {
                            bootstrapper.AddIoCRegistration(new InstanceTypeRegistration(options.SnapshotBehaviorProvider, typeof(ISnapshotBehaviorProvider)));
                            bootstrapper.AddIoCRegistration(new FactoryRegistration(
                                () => new MongoDbEventStore(options.SnapshotBehaviorProvider, options.SnapshotEventsArchiveBehavior),
                                typeof(MongoDbEventStore), typeof(IEventStore)));
                        }
                        else
                        {
                            EventStoreManager.SnapshotBehavior = options.SnapshotBehaviorProvider;
                        }
                    }
                    EventStoreManager.Activate();
                }
            };
            bootstrapper.AddService(service);
            return bootstrapper;
        }

        #endregion

    }
}
