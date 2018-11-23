using CQELight.Abstractions.DDD;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Tools.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.CosmosDb.Models
{
    internal class Snapshot : ISnapshot
    {

        #region Properties

        public Guid Id { get; set; }
        [JsonIgnore]
        public AggregateState AggregateState { get; set; }
        public string SnapshotData { get; set; }
        public string SnapshotBehaviorType { get; set; }
        public DateTime SnapshotTime { get; set; }
        public Guid AggregateId { get; set; }
        public string AggregateType { get; set; }

        #endregion

        #region Ctor

        private Snapshot() { }

        public Snapshot(Guid aggregateId, string aggregateType, AggregateState aggregateState, string snapshotBehaviorType, DateTime snapshotTime)
            : this(Guid.NewGuid(), aggregateId, aggregateType, aggregateState, snapshotBehaviorType, snapshotTime)
        {
        }

        public Snapshot(Guid id, Guid aggregateId, string aggregateType, AggregateState aggregateState, string snapshotBehaviorType, DateTime snapshotTime)
        {
            AggregateId = aggregateId;
            AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
            AggregateState = aggregateState ?? throw new ArgumentNullException(nameof(aggregateState));

            SnapshotBehaviorType = snapshotBehaviorType ?? throw new ArgumentNullException(nameof(snapshotBehaviorType));
            SnapshotTime = snapshotTime;

            SnapshotData = AggregateState.ToJson(true);

            Id = id;
        }

        #endregion

    }
}
