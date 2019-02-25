using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions.EventStore.Interfaces
{
    /// <summary>
    /// Contract interface for writing in event store.
    /// </summary>
    public interface IWriteEventStore
    {
        /// <summary>
        /// Store a domain event in the event store
        /// </summary>
        /// <param name="event">Event instance to be persisted.</param>
        /// <returns>A Result object that contains information if operation succeeded or not.</returns>
        Task<Result> StoreDomainEventAsync(IDomainEvent @event);
        /// <summary>
        /// Store a range of domain events in the event store.
        /// </summary>
        /// <param name="events">Collection of domain event to store.</param>
        /// <returns>A Result object that contains information if operation succeeded or not.</returns>
        Task<Result> StoreDomainEventRangeAsync(IEnumerable<IDomainEvent> events);
    }
}
