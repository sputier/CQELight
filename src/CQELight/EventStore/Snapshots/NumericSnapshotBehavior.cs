using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore;
using CQELight.Abstractions.EventStore.Interfaces.Snapshots;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CQELight.EventStore.Snapshots
{
    /// <summary>
    /// A simple numeric snapshot behavior.
    /// </summary>
    public class NumericSnapshotBehavior : IGenericSnapshotBehavior
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

        public (object AggregateState, IEnumerable<IDomainEvent> EventsToArchive) GenerateSnapshot<TAggregate, TId>(TAggregate rehydratedAggregate)
            where TAggregate : EventSourcedAggregate<TId>
        {
            var aggregateType = rehydratedAggregate.GetType();

            PropertyInfo stateProp = aggregateType.GetAllProperties().FirstOrDefault(p => p.PropertyType.IsSubclassOf(typeof(AggregateState)));
            FieldInfo stateField = aggregateType.GetAllFields().FirstOrDefault(f => f.FieldType.IsSubclassOf(typeof(AggregateState)));
            if (stateProp == null && stateField == null)
            {
                throw new InvalidOperationException("EFEventStore.GetRehydratedAggregateAsync() : " +
                    "Cannot find property/field that manage state for aggregate " +
                    $"type {aggregateType.FullName}. State should be a property or a field of the aggregate");
            }
            Type stateType = stateProp?.PropertyType ?? stateField?.FieldType;

            var currentState =
                (stateProp?.GetValue(rehydratedAggregate)
                ??
                stateField?.GetValue(rehydratedAggregate))
                as AggregateState;

            IEnumerable<IDomainEvent> snapshotEvents = Enumerable.Empty<IDomainEvent>();
            if (currentState.Events.All(e => e.Sequence != 0))
            {
                snapshotEvents = currentState.Events.OrderBy(e => e.Sequence).Take(_eventCount);
            }
            else
            {
                snapshotEvents = currentState.Events.OrderBy(e => e.EventTime).Take(_eventCount);
            }

            var stateObject = stateType.CreateInstance() as AggregateState;
            stateObject.ApplyRange(snapshotEvents);

            return (stateObject, snapshotEvents);
        }

        public bool IsSnapshotNeeded(IDomainEvent @event)
           => @event.Sequence > (ulong)_eventCount;

        #endregion

    }
}
