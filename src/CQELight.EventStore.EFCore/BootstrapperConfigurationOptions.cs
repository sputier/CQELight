using CQELight.Abstractions.EventStore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore
{
    /// <summary>
    /// Enumeration of available DbProvider for using EF Core as Event Store.
    /// </summary>
    public enum DbProvider
    {
        SQLServer,
        SQLite
    }
    /// <summary>
    /// Class that carries options for bootstrapper EF Core as Event Store.
    /// </summary>
    public class EFCoreEventStoreBootstrapperConfigurationOptions
    {

        #region Properties

        /// <summary>
        /// Plain-text connection string for database to target on.
        /// </summary>
        public string ConnectionString { get; }
        /// <summary>
        /// Current DbProvider to user to persist events.
        /// Defaults is SQL Server.
        /// </summary>
        public DbProvider Provider { get; set; } = DbProvider.SQLServer;
        /// <summary>
        /// Instance of snapshot behavior provider.
        /// </summary>
        public ISnapshotBehaviorProvider SnapshotBehaviorProvider { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new instance of the options class.
        /// </summary>
        /// <param name="connectionString">Value of the connection string. Required.</param>
        public EFCoreEventStoreBootstrapperConfigurationOptions(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("EFCoreEventStoreBootstrapperConfigurationOptions.ctor() : Connection string must be provided.",
                    nameof(connectionString));
            }

            ConnectionString = connectionString;
        }

        #endregion

    }
}
