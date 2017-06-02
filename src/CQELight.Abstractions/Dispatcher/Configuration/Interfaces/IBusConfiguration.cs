using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Dispatcher.Configuration.Interfaces
{
    /// <summary>
    /// Configuration to apply on bus.
    /// </summary>
    public interface IBusConfiguration
    {
        /// <summary>
        /// Create an error handle to fire if any exception happens on dispatch;
        /// </summary>
        /// <param name="handler">Handler to fire if exception..</param>
        /// <returns>Current configuration.</returns>
        IBusConfiguration HandleErrorWith(Action<Exception> handler);
        /// <summary>
        /// Specify the serializer for event transport.
        /// </summary>
        /// <typeparam name="T">Type of serializer to use.</typeparam>
        /// <returns>Current configuration.</returns>
        IBusConfiguration SerializeWith<T>() where T : class, IEventSerializer;
    }
}
