using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Dispatcher.Interfaces
{

    /// <summary>
    /// Contract interface for command buses than can be configured.
    /// </summary>
    /// <typeparam name="T">Type of configuration to apply.</typeparam>
    public interface IConfigurableCommandBus<T> where T : ICommandBusConfiguration
    {
        /// <summary>
        /// Apply the configuration to the bus.
        /// </summary>
        /// <param name="config">Bus configuration.</param>
        void Configure(T config);

    }
}
