using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Dispatcher.Interfaces
{
    /// <summary>
    /// Contract interface for event buses than can be configured.
    /// </summary>
    /// <typeparam name="T">Type of configuration to apply.</typeparam>
    public interface IConfigurableEventBus<T> where T : IDomainEventBusConfiguration
    {

        /// <summary>
        /// Apply the configuration to the bus.
        /// </summary>
        /// <param name="config">Bus configuration.</param>
        void Configure(T config);

    }
}
