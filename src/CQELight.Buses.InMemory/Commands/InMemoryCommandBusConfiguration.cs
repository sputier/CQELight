using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.InMemory.Commands
{
    /// <summary>
    /// In memory command bus configuration.
    /// </summary>
    public class InMemoryCommandBusConfiguration : ICommandBusConfiguration
    {

        #region Static properties

        /// <summary>
        /// Default configuration.
        /// </summary>
        public static InMemoryCommandBusConfiguration Default
            => new InMemoryCommandBusConfiguration(3, 500, null);


        #endregion

        #region Properties

        /// <summary>
        /// Waiting time between every try.
        /// </summary>
        public ulong WaitingTimeMilliseconds { get; private set; }
        /// <summary>
        /// Number of retries.
        /// </summary>
        public byte NbRetries { get; private set; }
        /// <summary>
        /// Callback to invoke when delivery failed.
        /// </summary>
        public Action<ICommand, ICommandContext> OnFailedDelivery { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Creation of a new configuration.
        /// </summary>
        /// <param name="nbRetries">Number of retries.</param>
        /// <param name="waitingTimeMilliseconds">Waiting time between every try.</param>
        /// <param name="onFailedDelivery">Callback to invoke when delivery failed.</param>
        public InMemoryCommandBusConfiguration(byte nbRetries, ulong waitingTimeMilliseconds,
            Action<ICommand, ICommandContext> onFailedDelivery)
        {
            WaitingTimeMilliseconds = waitingTimeMilliseconds;
            NbRetries = nbRetries;
            OnFailedDelivery = onFailedDelivery;
        }

        #endregion
    }
}
