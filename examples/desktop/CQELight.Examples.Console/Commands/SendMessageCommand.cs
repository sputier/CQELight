using CQELight.Abstractions.CQS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Examples.Console.Commands
{
    /// <summary>
    /// Simple send command DTO.
    /// The purpose of a command is to hold the data needed when hitting the domain.
    /// The command name is the domain action we want to proceed.
    /// </summary>
    public class SendMessageCommand : ICommand
    {
        #region Properties

        /// <summary>
        /// The message of command, which in our case, is the data.
        /// Setter should be private, the command is responsible of it's own integrety.
        /// </summary>
        public string Message { get; private set; }

        #endregion

        #region Ctor

        // Always keep a private parameterless constructor for serialization purposes.
        private SendMessageCommand()
        {

        }

        public SendMessageCommand(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("SendMessageCommand.ctor() : The message should be provided.", nameof(message));
            }

            Message = message;
        }

        #endregion

    }
}
