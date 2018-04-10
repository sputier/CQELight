using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions.EventStore.Interfaces
{
    /// <summary>
    /// Contract interface for event store.
    /// </summary>
    public interface IEventStore
    {

        /// <summary>
        /// Get a collection of events for a specific aggregate.
        /// </summary>
        /// <param name="aggregateUniqueId">Id of the aggregate which we want all the events.</param>
        /// <typeparam name="TAggregate">Aggregate type.</typeparam>
        /// <returns>Collection of all associated events.</returns>
        Task<IEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TAggregate>(Guid aggregateUniqueId);
        /// <summary>
        /// Get an event per its id.
        /// </summary>
        /// <param name="eventId">Id of the event.</param>
        /// <typeparam name="TEvent">Type of event to retrieve.</typeparam>
        /// <returns>Instance of the event.</returns>
        Task<TEvent> GetEventById<TEvent>(Guid eventId)
            where TEvent : class, IDomainEvent;

        /// <summary>
        /// Store a domain event in the event store
        /// </summary>
        /// <param name="event">Event instance to be persisted.</param>
        Task StoreDomainEventAsync(IDomainEvent @event);

    }
}
