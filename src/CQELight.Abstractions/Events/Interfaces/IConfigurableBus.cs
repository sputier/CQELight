using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Events.Interfaces
{
    /// <summary>
    /// Contract interface for buses than can be configured.
    /// </summary>
    /// <typeparam name="T">Type of configuration to apply.</typeparam>
    public interface IConfigurableBus<T> where T : IDomainEventBusConfiguration
    {

        /// <summary>
        /// Apply the configuration to the bus.
        /// </summary>
        /// <param name="config">Bus configuration.</param>
        void Configure(T config);

    }
}
