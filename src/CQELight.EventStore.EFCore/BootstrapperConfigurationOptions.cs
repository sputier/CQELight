using CQELight.Abstractions.EventStore.Interfaces;
using Microsoft.EntityFrameworkCore;
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
        /// Instance of snapshot behavior provider.
        /// </summary>
        public ISnapshotBehaviorProvider SnapshotBehaviorProvider { get; set; }
        /// <summary>
        /// Options for DbContext configuration.
        /// </summary>
        public DbContextOptions DbContextOptions { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new instance of the options class.
        /// </summary>
        /// <param name="dbContextOptions">Options for DbContext configuration</param>
        /// <param name="snapshotBehaviorProvider">Provider of snapshot behaviors</param>
        public EFCoreEventStoreBootstrapperConfigurationOptions(DbContextOptions dbContextOptions,
            ISnapshotBehaviorProvider snapshotBehaviorProvider = null)
        {
            DbContextOptions = dbContextOptions ?? throw new ArgumentNullException(nameof(dbContextOptions));
            SnapshotBehaviorProvider = snapshotBehaviorProvider;
        }

        #endregion

    }
}
