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
        /// Register asynchronously an event to be processed by the bus.
        /// </summary>
        /// <param name="event">Event to register.</param>
        /// <param name="context">Context associated to the event..</param>
        Task RegisterAsync(IDomainEvent @event, IEventContext context = null);
    }
}
