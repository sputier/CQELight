using System;
using System.Collections.Generic;
using System.Text;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.GregYoungsEventStore;
using CQELight.EventStore.EFCore;
using CQELight.EventStore.EFCore.Common;
using CQELight.IoC;

namespace CQELight
{
    public static class BootstrapperExt
    {
        #region Extension methods

        /// <summary>
        /// Use EF Core as EventStore for system, with the provided connection string.
        /// This is a usable case for common cases, in system that not have a huge amount of events, or for test/debug purpose.
        /// This is not recommanded for real-world big applications case.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="options">Options to use to configure event store.</param>
        /// <returns>Bootstrapper instance</returns>
        public static Bootstrapper UseGregYoungsEventStoreAsEventStore(this Bootstrapper bootstrapper,
            GregYoungsEventStoreConfiguration options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var service = new GregYoungsEventStoreBootstrappService
            {
                BootstrappAction = (ctx) =>
                {
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
