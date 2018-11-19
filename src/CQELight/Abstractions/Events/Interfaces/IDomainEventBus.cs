using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions.Events.Interfaces
{
    /// <summary>
    /// Contract interface for domain event bus dispatcher.
    /// </summary>
    public interface IDomainEventBus
    {
        /// <summary>
        /// Publish asynchronously an event to be processed by the bus.
        /// </summary>
        /// <param name="event">Event to register.</param>
        /// <param name="context">Context associated to the event..</param>
        Task PublishEventAsync(IDomainEvent @event, IEventContext context = null);
        /// <summary>
        /// Public asynchronously a bunch of events to be processed by the bus.
        /// </summary>
        /// <param name="data">Data that contains all events</param>
        Task PublishEventRangeAsync(IEnumerable<(IDomainEvent @event, IEventContext context)> data);
    }
}
