using CQELight.Abstractions.CQS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Dispatcher.Configuration.Commands.Interfaces
{
    /// <summary>
    /// Configuration for command dispatch.
    /// </summary>
    public interface ICommandConfiguration
    {
        /// <summary>
        /// Indicates a specific bus to use
        /// </summary>
        /// <typeparam name="T">Type of bus to use.</typeparam>
        /// <returns>Current configuration.</returns>
        ICommandDispatcherConfiguration UseBus<T>() where T : class, ICommandBus;
        /// <summary>
        /// Indicates to use all buses available within the system.
        /// </summary>
        /// <returns>Current configuration.</returns>
        ICommandDispatcherConfiguration UseAllAvailableBuses();
        /// <summary>
        /// Indicates to uses specified buses passed as parameter.
        /// </summary>
        /// <param name="types">Buses types to use.</param>
        /// <returns>Current configuration.</returns>
        ICommandDispatcherConfiguration UseBuses(params Type[] types);
    }
}
