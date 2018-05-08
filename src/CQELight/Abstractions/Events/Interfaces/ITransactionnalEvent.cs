using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace CQELight.Abstractions.Events.Interfaces
{
    /// <summary>
    /// Contract interface for transactionnal event.
    /// A transactionnel IS a domain event, but it carry the fact
    /// that all events should be treated as a transaction instead of individual events.
    /// </summary>
    public interface ITransactionnalEvent : IDomainEvent
    {
        /// <summary>
        /// Ordered collection of events of the transaction.
        /// </summary>
        ImmutableQueue<IDomainEvent> Events { get; }
    }
}
