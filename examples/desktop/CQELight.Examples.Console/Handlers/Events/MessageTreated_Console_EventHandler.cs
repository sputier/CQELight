using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Examples.Console.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Examples.Console.Handlers.Events
{
    public class MessageTreated_Console_EventHandler : IDomainEventHandler<MessageTreatedEvent>, IAutoRegisterType
    {
        public Task HandleAsync(MessageTreatedEvent domainEvent, IEventContext context = null)
        {
            Console.WriteLine($"Message ID {domainEvent.TreatedMessageId} : OK !");
            return Task.CompletedTask;
        }
    }
}
