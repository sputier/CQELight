using CQELight.Abstractions.Dispatcher.Configuration.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Dispatcher.Configuration.Events
{
    /// <summary>
    /// Configuration that applies to multiple event types.
    /// </summary>
    public class MultipleEventTypeConfiguration : IEventConfiguration, IEventDispatcherConfiguration
    {
        #region Members

        internal readonly IEnumerable<SingleEventTypeConfiguration> _eventTypesConfigs;

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="types">Types concerned by the configuration.</param>
        public MultipleEventTypeConfiguration(params Type[] types)
        {
            _eventTypesConfigs = types.Select(t => new SingleEventTypeConfiguration(t)).ToList();
        }

        #endregion

        #region  IEventConfiguration methods

        /// <summary>
        /// Set the flag 'SecurityCritical' on all events, which mean that they're cloned before being
        /// send to custom dispatcher callbacks.
        /// </summary>
        /// <returns>Current configuration.</returns>
        public IEventDispatcherConfiguration IsSecurityCritical()
        {
            _eventTypesConfigs.DoForEach(e => e.IsSecurityCritical());
            return this;
        }

        /// <summary>
        /// Create an error handle to fire if any exception happens on dispatch;
        /// </summary>
        /// <param name="handler">Handler to fire if exception..</param>
        /// <returns>Current configuration.</returns>
        public IEventDispatcherConfiguration HandleErrorWith(Action<Exception> handler)
        {
            _eventTypesConfigs.DoForEach(e => e.HandleErrorWith(handler));
            return this;
        }

        /// <summary>
        /// Specify the serializer for event transport.
        /// </summary>
        /// <typeparam name="T">Type of serializer to use.</typeparam>
        /// <returns>Current configuration.</returns>
        public IEventDispatcherConfiguration SerializeWith<T>() where T : class, IEventSerializer
        {
            _eventTypesConfigs.DoForEach(e => e.SerializeWith<T>());
            return this;
        }

        /// <summary>
        /// Indicates to use all buses available within the system.
        /// </summary>
        /// <returns>Current configuration.</returns>
        public IEventDispatcherConfiguration UseAllAvailableBuses()
        {
            _eventTypesConfigs.DoForEach(e => e.UseAllAvailableBuses());
            return this;
        }

        /// <summary>
        /// Indicates a specific bus to use
        /// </summary>
        /// <typeparam name="T">Type of bus to use.</typeparam>
        /// <returns>Current configuration.</returns>
        public IEventDispatcherConfiguration UseBus<T>() where T : class, IDomainEventBus
        {
            _eventTypesConfigs.DoForEach(e => e.UseBus<T>());
            return this;
        }

        /// <summary>
        /// Indicates to uses specified buses passed as parameter.
        /// </summary>
        /// <param name="types">Buses types to use.</param>
        /// <returns>Current configuration.</returns>
        public IEventDispatcherConfiguration UseBuses(params Type[] types)
        {
            _eventTypesConfigs.DoForEach(e => e.UseBuses(types));
            return this;
        }

        #endregion

    }
}
