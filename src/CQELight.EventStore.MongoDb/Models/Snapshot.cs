using CQELight.Abstractions.DDD;
using CQELight.Abstractions.EventStore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.MongoDb.Models
{
    internal class Snapshot : ISnapshot
    {

        #region Properties

        public Guid Id { get; private set; }
        public AggregateState AggregateState { get; private set; }
        public string SnapshotBehaviorType { get; private set; }
        public DateTime SnapshotTime { get; private set; }
        public object AggregateId{ get; private set; }
        public string AggregateType { get; private set; }

        #endregion

        #region Ctor

        public Snapshot(object aggregateId, string aggregateType, AggregateState aggregateState, string snapshotBehaviorType, DateTime snapshotTime)
            : this(Guid.NewGuid(), aggregateId, aggregateType, aggregateState, snapshotBehaviorType, snapshotTime)
        {
        }

        public Snapshot(Guid id, object aggregateId, string aggregateType, AggregateState aggregateState, string snapshotBehaviorType, DateTime snapshotTime)
        {
            AggregateId = aggregateId;
            AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
            AggregateState = aggregateState ?? throw new ArgumentNullException(nameof(aggregateState));

            SnapshotBehaviorType = snapshotBehaviorType ?? throw new ArgumentNullException(nameof(snapshotBehaviorType));
            SnapshotTime = snapshotTime;

            Id = id;
        }

        #endregion

    }
}
