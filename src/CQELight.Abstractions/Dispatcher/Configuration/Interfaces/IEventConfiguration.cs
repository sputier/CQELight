using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Dispatcher.Configuration.Interfaces
{
    /// <summary>
    /// Configuration for event dispatch.
    /// </summary>
    public interface IEventConfiguration
    {
        /// <summary>
        /// Indicates a specific bus to use
        /// </summary>
        /// <typeparam name="T">Type of bus to use.</typeparam>
        /// <returns>Current configuration.</returns>
        IBusConfiguration UseBus<T>() where T : class, IDomainEventBus;
        /// <summary>
        /// Indicates to use all buses available within the system.
        /// </summary>
        /// <returns>Current configuration.</returns>
        IBusConfiguration UseAllAvailableBuses();
        /// <summary>
        /// Indicates to uses specified buses passed as parameter.
        /// </summary>
        /// <param name="types">Buses types to use.</param>
        /// <returns>Current configuration.</returns>
        IBusConfiguration UseBuses(params Type[] types);
    }
}
