using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions
{
    /// <summary>
    /// Contract interface for Aggregates.
    /// </summary>
    public interface IAggregateRoot : IEntity
    {
        /// <summary>
        /// Collection of events of the Aggregate.
        /// </summary>
        IEnumerable<IDomainEvent> Events { get; }
    }
}
