using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Tools.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace CQELight.EventStore.Snapshots
{
    /// <summary>
    /// A simple numeric snapshot behavior.
    /// </summary>
    public class NumericSnapshotBehavior : ISnapshotBehavior
    {
        #region Members

        private readonly int _eventCount;

        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new NumericSnapshotBehavior with a defined count of events.
        /// </summary>
        /// <param name="eventCount">Number of events before snapshot.</param>
        public NumericSnapshotBehavior(int eventCount)
        {
            _eventCount = eventCount;
        }

        #endregion

        #region ISnapshotBehavior

        public (AggregateState AggregateStateToSnapshot, IEnumerable<IDomainEvent> EventsToArchive) GenerateSnapshot(AggregateState rehydratedAggregateState)
        {
            IEnumerable<IDomainEvent> snapshotEvents = Enumerable.Empty<IDomainEvent>();
            if (rehydratedAggregateState.Events.All(e => e.Sequence != 0))
            {
                snapshotEvents = rehydratedAggregateState.Events.OrderBy(e => e.Sequence).Take(_eventCount);
            }
            else
            {
                snapshotEvents = rehydratedAggregateState.Events.OrderBy(e => e.EventTime).Take(_eventCount);
            }

            var stateObject = rehydratedAggregateState.GetType().CreateInstance() as AggregateState;
            stateObject.ApplyRange(snapshotEvents);

            return (stateObject, snapshotEvents);
        }

        public bool IsSnapshotNeeded(IDomainEvent @event)
           => @event.Sequence > 1 && ((@event.Sequence - 1) % (ulong)_eventCount) == 0;

        #endregion

    }
}
