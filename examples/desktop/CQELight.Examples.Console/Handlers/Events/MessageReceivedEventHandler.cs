using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Examples.ConsoleApp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Examples.ConsoleApp.Handlers.Events
{
    /// <summary>
    /// Handle the received message event (the most important one in this system).
    /// We could simply add a way to save all received message, for example, by handling this event in a new handler.
    /// </summary>
    class MessageReceivedEventHandler : IDomainEventHandler<MessageSentEvent>, IAutoRegisterType
    {

        /// <summary>
        /// Handle the domain event.
        /// </summary>
        /// <param name="domainEvent">Domain event to handle.</param>
        /// <param name="context">Associated context.</param>
        public Task HandleAsync(MessageSentEvent domainEvent, IEventContext context = null)
        {
            if (domainEvent.SenderName != Program.FriendlyName)
                Console.WriteLine($"{domainEvent.SenderName} >> {domainEvent.Message}");
            return Task.CompletedTask;
        }
    }
}
