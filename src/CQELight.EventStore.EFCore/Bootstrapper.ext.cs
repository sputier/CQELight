﻿using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.EFCore;
using CQELight.EventStore.EFCore.Common;
using CQELight.IoC;
using CQELight.Tools.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight
{
    public static class BootstrapperExt
    {
        #region Private class

        private class EFEventStoreBootstrappService : IBootstrapperService
        {
            public BootstrapperServiceType ServiceType => BootstrapperServiceType.EventStore;
            public Action<BootstrappingContext> BootstrappAction { get; internal set; }
        }

        #endregion

        #region Extension methods

        /// <summary>
        /// Use EF Core as EventStore for system, with the provided connection string.
        /// This is a usable case for common cases, in system that not have a huge amount of events, or for test/debug purpose.
        /// This is not recommanded for real-world big applications case.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="options">Options to use to configure event store.</param>
        /// <returns>Bootstrapper instance</returns>
        public static Bootstrapper UseEFCoreAsEventStore(this Bootstrapper bootstrapper,
            EFCoreEventStoreBootstrapperConfigurationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var service = new EFEventStoreBootstrappService
            {
                BootstrappAction = (ctx) =>
                {
                    bootstrapper.AddIoCRegistration(new FactoryRegistration(() =>
                        new EventStoreDbContext(options.DbContextOptions, options.ArchiveBehavior), typeof(EventStoreDbContext)));
                    if (options.SnapshotBehaviorProvider != null)
                    {
                        if (ctx.IsServiceRegistered(BootstrapperServiceType.IoC))
                        {
                            bootstrapper.AddIoCRegistration(
                                new InstanceTypeRegistration(options.SnapshotBehaviorProvider, typeof(ISnapshotBehaviorProvider)));
                        }
                        else
                        {
                            EventStoreManager.SnapshotBehaviorProvider = options.SnapshotBehaviorProvider;
                        }
                    }
                    EventStoreManager.DbContextOptions = options.DbContextOptions;
                    EventStoreManager.BufferInfo = options.BufferInfo;
                    if (options.ArchiveDbContextOptions != null)
                    {
                        EventStoreManager.ArchiveBehaviorInfos = new EventArchiveBehaviorInfos
                        {
                            ArchiveBehavior = options.ArchiveBehavior,
                            ArchiveDbContextOptions = options.ArchiveDbContextOptions
                        };
                        if (options.ArchiveBehavior == EventStore.SnapshotEventsArchiveBehavior.StoreToNewDatabase)
                        {
                            bootstrapper.AddIoCRegistration(new FactoryRegistration(() =>
                                new ArchiveEventStoreDbContext(options.ArchiveDbContextOptions), typeof(ArchiveEventStoreDbContext)));
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
