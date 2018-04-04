using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.InMemory.Events
{
    /// <summary>
    /// A configuration builder for in-memory event bus.
    /// </summary>
    public class InMemoryEventBusConfigurationBuilder
    {

        #region Members

        private InMemoryEventBusConfiguration _config = new InMemoryEventBusConfiguration();

        #endregion

        #region Public methods

        /// <summary>
        /// Set a retrying strategy for configuration, based on time between retries and
        /// a number of retries.
        /// </summary>
        /// <param name="timeoutBetweenTries">Time between each retries.</param>
        /// <param name="nbRetries">Number of total retries.</param>
        /// <returns>Current configuration.</returns>
        public InMemoryEventBusConfigurationBuilder SetRetryStrategy(ulong timeoutBetweenTries, byte nbRetries)
        {
            _config.WaitingTimeMilliseconds = timeoutBetweenTries;
            _config.NbRetries = nbRetries;
            return this;
        }

        /// <summary>
        /// Defines a callback to invoke when a dispatch exception is thrown.
        /// </summary>
        /// <param name="callback">Callback method</param>
        /// <returns>Current configuration.</returns>
        public InMemoryEventBusConfigurationBuilder DefineErrorCallback(Action<IDomainEvent, IEventContext> callback)
        {
            _config.OnFailedDelivery = callback;
            return this;
        }

        /// <summary>
        /// Retrieve the build configuration.
        /// </summary>
        /// <returns>Instance of configuration.</returns>
        public InMemoryEventBusConfiguration Build()
            => _config;

        #endregion

    }
}
