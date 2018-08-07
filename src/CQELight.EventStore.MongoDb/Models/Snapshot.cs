using CQELight.Abstractions.EventStore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.MongoDb.Models
{
    internal class Snapshot : ISnapshot
    {
        public IEventSourcedAggregate Aggregate { get; private set; }
        public string SnapshotBehaviorType { get; private set; }
        public DateTime SnapshotTime { get; private set; }

        public Guid AggregateId { get; private set; }

        public string AggregateType { get; private set; }

        public Snapshot(Guid aggregateId, string aggregateType, IEventSourcedAggregate aggregate, string snapshotBehaviorType,DateTime snapshotTime)
        {
            AggregateId = aggregateId;
            AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
            Aggregate = aggregate ?? throw new ArgumentNullException(nameof(aggregate));

            SnapshotBehaviorType = snapshotBehaviorType ?? throw new ArgumentNullException(nameof(snapshotBehaviorType));
            SnapshotTime = snapshotTime;
        }
    }
}
