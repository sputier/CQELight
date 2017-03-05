using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions
{
    /// <summary>
    /// Contract interface for domain event bus dispatcher.
    /// </summary>
    public interface IDomainEventBus
    {
        /// <summary>
        /// Dispatch events asynchronously to all listening handlers.
        /// </summary>
        /// <param name="events">Events to dispatch.</param>
        Task DispatchAsync(IEnumerable<IDomainEvent> events);

    }
}
