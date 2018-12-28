using CQELight.Abstractions.DDD;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.EventStore.Interfaces
{
    /// <summary>
    /// Base type interface for defining a snapshot.
    /// </summary>
    public interface ISnapshot
    {
        /// <summary>
        /// Unique Id of the snapshot
        /// </summary>
        Guid Id { get; }
        /// <summary>
        /// HashedAggregateId on which the snapshot is done.
        /// </summary>
        int HashedAggregateId { get; }
        /// <summary>
        /// Type of aggregate that snapshot is concerned.
        /// </summary>
        string AggregateType { get; }
        /// <summary>
        /// State of the aggregate that should be considered for snapshoting
        /// </summary>
        AggregateState AggregateState { get; }
        /// <summary>
        /// Type of generated snapshot.
        /// </summary>
        string SnapshotBehaviorType { get;  }
        /// <summary>
        /// Time when the snapshot occured.
        /// </summary>
        DateTime SnapshotTime { get; }

    }
}
