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
    public interface IAggregateSnapshotBehavior<T, TId> : ISnapshotBehavior
        where T : EventSourcedAggregate<TId>
    {
        /// <summary>
        /// Generate the result of computation that event store will have to use
        /// to snapshot.
        /// </summary>
        /// <param name="rehydratedAggregate">Up to date aggregate instance</param>
        /// <returns>A new snapshot instance and the collection of events to archive.</returns>
        (object Snapshot, IEnumerable<IDomainEvent> ArchiveEvents) GenerateSnapshot(T rehydratedAggregate);

    }
}
