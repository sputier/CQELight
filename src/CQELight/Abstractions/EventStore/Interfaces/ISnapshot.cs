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
        /// AggregateId on which the snapshot is done.
        /// </summary>
        Guid AggregateId { get; }
        /// <summary>
        /// Type of aggregate that snapshot is concerned.
        /// </summary>
        string AggregateType { get; }

    }
}
