using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Implementations.Events.InMemory.Stateless
{
    /// <summary>
    /// Configuration class for InMemoryStatelessEventBus
    /// </summary>
    public class InMemoryStatelessEventBusConfiguration : IDomainEventBusConfiguration
    {

        #region Static properties

        /// <summary>
        /// Default configuration.
        /// </summary>
        public static InMemoryStatelessEventBusConfiguration Default
            => new InMemoryStatelessEventBusConfiguration(500);

        #endregion

        #region Properties

        /// <summary>
        /// Waiting time between each event dispatch.
        /// </summary>
        public uint WaitTime { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InMemoryStatelessEventBusConfiguration()
            : this(500)
        {

        }

        /// <summary>
        /// Constructor with waitingTime.
        /// </summary>
        /// <param name="waitTime">Wainting time.</param>
        public InMemoryStatelessEventBusConfiguration(uint waitTime)
        {
            WaitTime = waitTime;
        }

        #endregion
    }
}
