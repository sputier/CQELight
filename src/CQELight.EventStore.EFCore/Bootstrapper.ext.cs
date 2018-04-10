using CQELight.EventStore.EFCore.Common;
using CQELight.IoC;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore
{

    //Because of EFCore way of genering migration, which is not database agnostic, we're forced to use the EnsureCreated method, which is incompatible
    //with model update. If model should be updated in next future, we will have to handle migration by ourselves, unless EF Core provides a migraton
    //which is database agnostic

    public static class BootstrapperExt
    {

        #region Extension methods

        /// <summary>
        /// Use SQLServer as EventStore for system, with the provided connection string.
        /// This is a usable case for common cases, in system that not have a huge amount of events, or for test/debug purpose.
        /// This is not recommanded for real-world big applications case.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="connectionString">Connection string to SQL Server.</param>
        /// <returns>Bootstrapper instance</returns>
        public static Bootstrapper UseSQLServerWithEFCoreAsEventStore(this Bootstrapper bootstrapper, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Bootstrapper.UseSQLServerWithEFCoreAsEventStore() : Connection string must be provided.", nameof(connectionString));
            }
            AddDbContextRegistration(bootstrapper, connectionString);
            EventStoreManager.Activate();

            return bootstrapper;
        }

        /// <summary>
        /// Use SQLite as EventStore for system, with the provided connection string.
        /// This is only recommanded for cases where SQLite is the only usable solution, such as mobile cases.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="connectionString">Connection string to SQLite.</param>
        /// <returns>Bootstrapper instance</returns>
        public static Bootstrapper UseSQLiteWithEFCoreAsEventStore(this Bootstrapper bootstrapper, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Bootstrapper.UseSQLiteWithEFCoreAsEventStore() : Connection string should be provided.", nameof(connectionString));
            }
            AddDbContextRegistration(bootstrapper, connectionString, false);
            EventStoreManager.Activate();

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
