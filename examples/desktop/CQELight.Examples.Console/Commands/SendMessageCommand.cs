using CQELight.Abstractions.CQS.Interfaces;
using System;

namespace CQELight.Examples.ConsoleApp.Commands
{
    /// <summary>
    /// Simple command to send message to all chat users.
    /// </summary>

    public class SendMessageCommand : ICommand
    {

        #region Properties

        /// <summary>
        /// Message to send.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Specific receiver.
        /// </summary>
        public Guid? To { get; set; }


        #endregion

        #region Ctor

        /// <summary>
        /// Default ctor.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="to">Specific receiver.</param>
        public SendMessageCommand(string message, Guid? to = null)
        {
            Message = message;
            To = to;
        }

        #endregion

    }
}
