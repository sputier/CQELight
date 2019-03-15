using CQELight.Abstractions.DDD;
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
        /// Get the info if a snapshot is needed, based on the current event.
        /// </summary>
        /// <param name="event">Current event that is actually being stored.</param>
        /// <returns>True if a snapshot should be created, false otherwise.</returns>
        bool IsSnapshotNeeded(IDomainEvent @event);
        /// <summary>
        /// Generate the result of computation that event store will have to use
        /// to snapshot.
        /// </summary>
        /// <param name="rehydratedAggregateState">Up to date aggregate state instance</param>
        /// <returns>The collection to event to archive</returns>
        IEnumerable<IDomainEvent> GenerateSnapshot(AggregateState rehydratedAggregateState);
    }
}
