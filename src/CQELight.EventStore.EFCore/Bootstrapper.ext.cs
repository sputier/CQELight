using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.EFCore;
using CQELight.EventStore.EFCore.Common;
using CQELight.IoC;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

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
                    AddDbContextRegistration(bootstrapper, options.ConnectionString, options.Provider == DbProvider.SQLServer);
                    if (options.SnapshotBehaviorProvider != null)
                    {
                        if (ctx.IsServiceRegistered(BootstrapperServiceType.IoC))
                        {
                            bootstrapper.AddIoCRegistration(new InstanceTypeRegistration(options.SnapshotBehaviorProvider, typeof(ISnapshotBehaviorProvider)));
                        }
                        else
                        {
                            EventStoreManager.SnapshotBehaviorProvider = options.SnapshotBehaviorProvider;
                        }
                    }
                    EventStoreManager.Activate();
                }
            };
            bootstrapper.AddService(service);
            return bootstrapper;
        }

        #endregion

        #region Private methods

        private static void AddDbContextRegistration(Bootstrapper bootstrapper, string connectionString, bool sqlServer = true)
        {
            var ctxConfig = new DbContextConfiguration
            {
                ConfigType = sqlServer ? ConfigurationType.SQLServer : ConfigurationType.SQLite,
                ConnectionString = connectionString
            };
            bootstrapper.AddIoCRegistration(new InstanceTypeRegistration(new EventStoreDbContext(ctxConfig), typeof(EventStoreDbContext)));
            //if ioc not used
            EventStoreManager.DbContextConfiguration = ctxConfig;
        }

        #endregion

    }
}
