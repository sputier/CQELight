using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions.EventStore.Interfaces
{
    /// <summary>
    /// Contract interface for making snapshot upon an event store.
    /// </summary>
    public interface ISnapshotBehavior
    {

        /// <summary>
        /// Generate a new snapshot based on the aggregate id and the aggregate type.
        /// </summary>
        /// <param name="aggregateId">Value of the id of the aggregate.</param>
        /// <param name="aggregateType">Type of the aggregate.</param>
        /// <param name="rehydratedAggregate">Up to date aggregate instance</param>
        /// <returns>A new snapshot instance and the collection of events to archive.</returns>
        Task<(ISnapshot Snapshot,  IEnumerable<IDomainEvent> ArchiveEvents)> 
            GenerateSnapshotAsync(object aggregateId, Type aggregateType, IEventSourcedAggregate rehydratedAggregate);

        /// <summary>
        /// Get the info if a snapshot is needed, based on the aggregate id and the aggregate type.
        /// </summary>
        /// <param name="aggregateId">Value of the id of the aggregate.</param>
        /// <param name="aggregateType">Type of the aggregate.</param>
        /// <returns>True if a snapshot should be created, false otherwise.</returns>
        Task<bool> IsSnapshotNeededAsync(object aggregateId, Type aggregateType);
    }
}
