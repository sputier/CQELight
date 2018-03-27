using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions.Dispatcher.Interfaces
{
    /// <summary>
    /// Interface for message handlers.
    /// </summary>
    /// <typeparam name="T">Type of message to handle.</typeparam>
    public interface IMessageHandler<T>
        where T : IMessage
    {

        /// <summary>
        /// Base method to handle asynchronously the message.
        /// </summary>
        /// <param name="message">Instance of received message</param>
        Task HandleMessageAsync(T message);

    }
}
