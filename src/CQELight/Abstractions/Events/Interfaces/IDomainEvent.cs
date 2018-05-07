using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Events.Interfaces
{
    /// <summary>
    /// A public class that represents a Domain Event
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Unique id of the event.
        /// </summary>
        Guid Id { get; }
        /// <summary>
        /// Time when event happens.
        /// </summary>
        DateTime EventTime { get; }
        /// <summary>
        /// Linked aggregate Id if any.
        /// </summary>
        Guid? AggregateId { get; }
        /// <summary>
        /// Type of aggregate linked to event.
        /// </summary>
        Type AggregateType{ get; }
        /// <summary>
        /// Current sequence within aggregate's events chain.
        /// </summary>
        ulong Sequence { get; }
    }
}
