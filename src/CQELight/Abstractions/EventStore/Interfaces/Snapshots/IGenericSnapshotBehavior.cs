using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.EventStore.Interfaces.Snapshots
{
    /// <summary>
    /// Base contract interface for generic snapshot behavior, that can
    /// apply to all kind of aggregates.
    /// </summary>
    public interface IGenericSnapshotBehavior : ISnapshotBehavior
    {
        /// <summary>
        /// Generate the result of computation that event store will have to use
        /// to snapshot.
        /// </summary>
        /// <param name="rehydratedAggregate">Up to date aggregate instance</param>
        /// <returns>A dual value that contains the </returns>
        (object AggregateState, IEnumerable<IDomainEvent> EventsToArchive) GenerateSnapshot<TAggregate, TId>(TAggregate rehydratedAggregate)
            where TAggregate : EventSourcedAggregate<TId>;
    }
}
