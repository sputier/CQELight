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
            => new InMemoryCommandBusConfiguration(null);


        #endregion

        #region Properties

        /// <summary>
        /// Callback when no handler for a specific command is found in the same process.
        /// </summary>
        public Action<ICommand, ICommandContext> OnNoHandlerFounds { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Creation of a new configuration.
        /// </summary>
        /// <param name="onFailedDelivery">Callback to invoke when delivery failed.</param>
        public InMemoryCommandBusConfiguration(Action<ICommand, ICommandContext> onNoHandlerFounds)
        {
            OnNoHandlerFounds = onNoHandlerFounds;
        }

        #endregion
    }
}
