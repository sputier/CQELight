using CQELight.Abstractions.CQS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Dispatcher.Configuration.Commands.Interfaces
{
    /// <summary>
    /// Contract interface for command dispatching.
    /// </summary>
    public interface ICommandDispatcherConfiguration
    {
        /// <summary>
        /// Create an error handle to fire if any exception happens on dispatch;
        /// </summary>
        /// <param name="handler">Handler to fire if exception.</param>
        /// <returns>Current configuration.</returns>
        ICommandDispatcherConfiguration HandleErrorWith(Action<Exception> handler);
        /// <summary>
        /// Specify the serializer for command transport.
        /// </summary>
        /// <typeparam name="T">Type of serializer to use.</typeparam>
        /// <returns>Current configuration.</returns>
        ICommandDispatcherConfiguration SerializeWith<T>() where T : class, ICommandSerializer;
        /// <summary>
        /// Set the 'SecurityCritical' flag on this command type to clone it before sending it to
        /// dispatcher custom callbacks. It's more secure because instance cannot be modified, 
        /// but it's slower.
        /// </summary>
        /// <returns>Current configuration</returns>
        ICommandDispatcherConfiguration IsSecurityCritical();
    }
}
