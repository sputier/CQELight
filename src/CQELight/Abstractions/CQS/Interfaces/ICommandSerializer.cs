using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.CQS.Interfaces
{
    /// <summary>
    /// Contract interface for command serializing.
    /// </summary>
    public interface ICommandSerializer
    {
        /// <summary>
        /// Serialize a command to a string value.
        /// </summary>
        /// <param name="command">Command to serialize.</param>
        /// <returns>Command that has been serialized into a string.</returns>
        string SerializeCommand(ICommand command);
        /// <summary>
        /// Deserialize a command from string.
        /// </summary>
        /// <param name="data">String data that contains serialized command.</param>
        /// <returns>Instance of command.</returns>
        ICommand DeserializeCommand(string data);
        /// <summary>
        /// Deserialize a command from string.
        /// </summary>
        /// <param name="data">String data that contains serialized command.</param>
        /// <typeparam name="T">Type of command to obtain.</typeparam>
        /// <returns>Instance of command.</returns>
        T DeserializeCommand<T>(string data)
            where T : ICommand;
    }
}
