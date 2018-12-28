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
        public int HashedAggregateId { get; private set; }
        public string AggregateType { get; private set; }

        #endregion

        #region Ctor

        public Snapshot(int hashedAggregateId, string aggregateType, AggregateState aggregateState, string snapshotBehaviorType, DateTime snapshotTime)
            : this(Guid.NewGuid(), hashedAggregateId, aggregateType, aggregateState, snapshotBehaviorType, snapshotTime)
        {
        }

        public Snapshot(Guid id, int hashedAggregateId, string aggregateType, AggregateState aggregateState, string snapshotBehaviorType, DateTime snapshotTime)
        {
            HashedAggregateId = hashedAggregateId;
            AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
            AggregateState = aggregateState ?? throw new ArgumentNullException(nameof(aggregateState));

            SnapshotBehaviorType = snapshotBehaviorType ?? throw new ArgumentNullException(nameof(snapshotBehaviorType));
            SnapshotTime = snapshotTime;

            Id = id;
        }

        #endregion

    }
}
