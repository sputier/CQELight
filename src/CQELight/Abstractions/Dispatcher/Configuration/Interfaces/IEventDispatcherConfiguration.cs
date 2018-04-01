using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Dispatcher.Configuration.Interfaces
{
    /// <summary>
    /// Contract interface for event dispatching.
    /// </summary>
    public interface IEventDispatcherConfiguration
    {
        /// <summary>
        /// Create an error handle to fire if any exception happens on dispatch;
        /// </summary>
        /// <param name="handler">Handler to fire if exception.</param>
        /// <returns>Current configuration.</returns>
        IEventDispatcherConfiguration HandleErrorWith(Action<Exception> handler);
        /// <summary>
        /// Specify the serializer for event transport.
        /// </summary>
        /// <typeparam name="T">Type of serializer to use.</typeparam>
        /// <returns>Current configuration.</returns>
        IEventDispatcherConfiguration SerializeWith<T>() where T : class, IEventSerializer;
        /// <summary>
        /// Set the 'SecurityCritical' flag on this event type to clone it before sending it to
        /// dispatcher custom callbacks. It's more secure because instance cannot be modified, 
        /// but it's slower.
        /// </summary>
        /// <returns>Current configuration</returns>
        IEventDispatcherConfiguration IsSecurityCritical();
    }
}
