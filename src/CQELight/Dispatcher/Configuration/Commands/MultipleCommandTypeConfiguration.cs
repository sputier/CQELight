using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Dispatcher.Configuration.Commands.Interfaces;
using CQELight.Abstractions.Dispatcher.Configuration.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Dispatcher.Configuration.Commands
{
    /// <summary>
    /// Configuration that applies to multiple command types.
    /// </summary>
    public class MultipleCommandTypeConfiguration : ICommandConfiguration, ICommandDispatcherConfiguration
    {
        #region Members

        internal readonly IEnumerable<SingleCommandTypeConfiguration> _commandTypesConfigs;

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="types">Types concerned by the configuration.</param>
        public MultipleCommandTypeConfiguration(params Type[] types)
        {
            _commandTypesConfigs = types.Select(t => new SingleCommandTypeConfiguration(t)).ToList();
        }

        #endregion

        #region  ICommandDispatcherConfiguration methods

        /// <summary>
        /// Set the flag 'SecurityCritical' on all events, which mean that they're cloned before being
        /// send to custom dispatcher callbacks.
        /// </summary>
        /// <returns>Current configuration.</returns>
        public ICommandDispatcherConfiguration IsSecurityCritical()
        {
            _commandTypesConfigs.DoForEach(e => e.IsSecurityCritical());
            return this;
        }

        /// <summary>
        /// Create an error handle to fire if any exception happens on dispatch;
        /// </summary>
        /// <param name="handler">Handler to fire if exception..</param>
        /// <returns>Current configuration.</returns>
        public ICommandDispatcherConfiguration HandleErrorWith(Action<Exception> handler)
        {
            _commandTypesConfigs.DoForEach(e => e.HandleErrorWith(handler));
            return this;
        }

        /// <summary>
        /// Specify the serializer for event transport.
        /// </summary>
        /// <typeparam name="T">Type of serializer to use.</typeparam>
        /// <returns>Current configuration.</returns>
        public ICommandDispatcherConfiguration SerializeWith<T>() where T : class, ICommandSerializer
        {
            _commandTypesConfigs.DoForEach(e => e.SerializeWith<T>());
            return this;
        }

        /// <summary>
        /// Indicates to use all buses available within the system.
        /// </summary>
        /// <returns>Current configuration.</returns>
        public ICommandDispatcherConfiguration UseAllAvailableBuses()
        {
            _commandTypesConfigs.DoForEach(e => e.UseAllAvailableBuses());
            return this;
        }

        /// <summary>
        /// Indicates a specific bus to use
        /// </summary>
        /// <typeparam name="T">Type of bus to use.</typeparam>
        /// <returns>Current configuration.</returns>
        public ICommandDispatcherConfiguration UseBus<T>() where T : class, ICommandBus
        {
            _commandTypesConfigs.DoForEach(e => e.UseBus<T>());
            return this;
        }

        /// <summary>
        /// Indicates to uses specified buses passed as parameter.
        /// </summary>
        /// <param name="types">Buses types to use.</param>
        /// <returns>Current configuration.</returns>
        public ICommandDispatcherConfiguration UseBuses(params Type[] types)
        {
            _commandTypesConfigs.DoForEach(e => e.UseBuses(types));
            return this;
        }

        #endregion

    }
}
