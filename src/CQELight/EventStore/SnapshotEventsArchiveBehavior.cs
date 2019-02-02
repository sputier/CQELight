using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore
{
    /// <summary>
    /// Behavior to use when extracting events
    /// for generating a snapshot. Even if Delete option
    /// is provided, it should be avoided as much as possible,
    /// cause it alterates the principle of an EventStore. Use it
    /// ONLY IF you have disk space cost issues.
    /// </summary>
    public enum SnapshotEventsArchiveBehavior
    {
        /// <summary>
        /// Use this option to completely disable snapshot behavior.
        /// Note that event if a snapshot provider is provided later on,
        /// it will not be used.
        /// </summary>
        Disabled,
        /// <summary>
        /// Store archive events to a new database.
        /// Default and prefered value for both performance
        /// and business logic.
        /// </summary>
        StoreToNewDatabase,
        /// <summary>
        /// Store archive events to a new table within the same 
        /// database. Use it if you have no rights to create
        /// new databases
        /// </summary>
        StoreToNewTable,
        /// <summary>
        /// Delete all archive events. This option should be 
        /// avoided and ONLY used if disk space cost 
        /// is a REAL issue for your company.
        /// </summary>
        Delete
    }
}
