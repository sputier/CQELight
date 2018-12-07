using CQELight.Abstractions.EventStore.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore
{


    /// <summary>
    /// Class that carries options for bootstrapper EF Core as Event Store.
    /// </summary>
    public class EFCoreEventStoreBootstrapperConfigurationOptions
    {

        #region Properties

        /// <summary>
        /// Instance of snapshot behavior provider.
        /// </summary>
        public ISnapshotBehaviorProvider SnapshotBehaviorProvider { get; }
        /// <summary>
        /// Options for DbContext configuration.
        /// </summary>
        public DbContextOptions DbContextOptions { get; }
        /// <summary>
        /// Informations about using buffer or not.
        /// </summary>
        public BufferInfo BufferInfo { get; }
        /// <summary>
        /// Event archive behavior to apply when generating 
        /// snapshot.
        /// </summary>
        public SnapshotEventsArchiveBehavior ArchiveBehavior { get; }
        /// <summary>
        /// DbContext options for archive behavior.
        /// </summary>
        public DbContextOptions ArchiveDbContextOptions { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new instance of the options class.
        /// </summary>
        /// <param name="dbContextOptions">Options for DbContext configuration</param>
        /// <param name="snapshotBehaviorProvider">Provider of snapshot behaviors</param>
        /// <param name="bufferInfo">Buffer info to use. Disabled by default.</param>
        /// <param name="archiveBehavior">Behavior to adopt when creating a snapshot</param>
        /// <param name="archiveDbContextOptions">
        /// Options to access event archive database.
        /// A value is needed if <paramref name="snapshotBehaviorProvider"/> is provided and
        /// <paramref name="archiveBehavior"/> is set to StoreToNewDatabase.
        /// </param>
        public EFCoreEventStoreBootstrapperConfigurationOptions(DbContextOptions dbContextOptions,
            ISnapshotBehaviorProvider snapshotBehaviorProvider = null,
            BufferInfo bufferInfo = null,
            SnapshotEventsArchiveBehavior archiveBehavior = SnapshotEventsArchiveBehavior.StoreToNewDatabase,
            DbContextOptions archiveDbContextOptions = null)
        {
            DbContextOptions = dbContextOptions ?? throw new ArgumentNullException(nameof(dbContextOptions));
            SnapshotBehaviorProvider = snapshotBehaviorProvider;
            BufferInfo = bufferInfo ?? BufferInfo.Disabled;
            if (snapshotBehaviorProvider != null)
            {
                ArchiveBehavior = archiveBehavior;
                ArchiveDbContextOptions = archiveDbContextOptions;
                if (archiveBehavior == SnapshotEventsArchiveBehavior.StoreToNewDatabase && archiveDbContextOptions == null)
                {
                    throw new ArgumentException("A DbContextOptions should be provided to access archive database cause " +
                        "SnapshotEventsArchiveBehavior is set to StoreToNewDatabase.");
                }
            }
        }

        #endregion

    }
}
