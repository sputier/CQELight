using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher;
using CQELight.Examples.ConsoleApp.Commands;
using CQELight.Examples.ConsoleApp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Examples.ConsoleApp.Handlers.Commands
{
    /// <summary>
    /// Command handler for the send message command.
    /// This handler will be invoke with a command that contains a message
    /// that needs to be send into the system.
    /// </summary>
    class SendMessageCommandHandler : ICommandHandler<SendMessageCommand>, IAutoRegisterType
    {
        /// <summary>
        /// Handle a specific command instance with its context.
        /// </summary>
        /// <param name="command">Command to handle.</param>
        /// <param name="context">Linked context.</param>
        public async Task HandleAsync(SendMessageCommand command, ICommandContext context = null)
        {
            //Sent the event that message has been treated and is going to be send.
            if (command.To != Program.Id) // Don't sent message to ourselves
            {
                await CoreDispatcher.DispatchEventAsync(new MessageSentEvent { Message = command.Message, SenderName = Program.FriendlyName });
            }
        }
    }
}
