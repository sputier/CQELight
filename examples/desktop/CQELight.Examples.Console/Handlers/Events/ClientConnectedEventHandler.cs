using CQELight.Abstractions.Events.Interfaces;
using CQELight.Examples.ConsoleApp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Examples.ConsoleApp.Handlers.Events
{
    /// <summary>
    /// Handle the event of a client getting connected.
    /// When this is behavior is tested and accepted, we shouldn't modify it.
    /// Instead, we should create a fresh new IDomainEventHandler with our new functionnality.
    /// </summary>
    class ClientConnectedEventHandler : IDomainEventHandler<ClientConnectedEvent>
    {

        /// <summary>
        /// Handle the domain event.
        /// </summary>
        /// <param name="domainEvent">Domain event to handle.</param>
        /// <param name="context">Associated context.</param>
        public Task HandleAsync(ClientConnectedEvent domainEvent, IEventContext context = null)
        {
            if (domainEvent.FriendlyName != Program.FriendlyName)
            {
                Console.WriteLine($"{domainEvent.FriendlyName} is connected!");
            }
            return Task.CompletedTask;
        }
    }
}
