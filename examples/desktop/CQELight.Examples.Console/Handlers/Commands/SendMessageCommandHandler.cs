using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher;
using CQELight.Examples.Console.Commands;
using CQELight.Examples.Console.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Examples.Console.Handlers.Commands
{
    /// <summary>
    /// This class act as handler when command is of type 'SendMessageCommand' is send into the system.
    /// It will be retrieved from ioc container (if any), or instantiate through reflexion.
    /// By setting IAutoRegisterType, the type will be automatically registered in the IoC container.
    /// </summary>
    public class SendMessageCommandHandler : ICommandHandler<SendMessageCommand>, IAutoRegisterType
    {
        /// <summary>
        /// This is the main asynchronous method that get called when handler is created and should be invoked.
        /// </summary>
        public async Task HandleAsync(SendMessageCommand command, ICommandContext context = null)
        {
            // Act with your business logic.
            // Command handler should handle infrastructural issues to keep domain pure.

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"New message received : {command.Message}");
            Console.ForegroundColor = ConsoleColor.White;

            await CoreDispatcher.DispatchEventAsync(new MessageTreatedEvent(Guid.NewGuid(), command.Message));
        }
    }
}
