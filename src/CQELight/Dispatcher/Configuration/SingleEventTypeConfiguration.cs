using CQELight.Abstractions.Dispatcher.Configuration.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Dispatcher.Configuration.Internal;
using CQELight.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQELight.Dispatcher.Configuration
{
    /// <summary>
    /// Create a configuration for a single event type.
    /// </summary>
    public class SingleEventTypeConfiguration : IEventConfiguration, IBusConfiguration
    {
        
        #region Members

        /// <summary>
        /// Current event bus configs.
        /// </summary>
        internal ICollection<EventDispatchConfigurationBuilder> _busConfigs;
        /// <summary>
        /// Current config to manage.
        /// </summary>
        private EventDispatchConfigurationBuilder _currentConfig;
        /// <summary>
        /// Type of event for the configuration.
        /// </summary>
        internal Type _eventType;

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="eventType">Type of event to configure.</param>
        public SingleEventTypeConfiguration(Type eventType)
        {
            _busConfigs = new List<EventDispatchConfigurationBuilder>();
            _eventType = eventType;
        }

        #endregion

        #region IEventConfiguration methods
        
        /// <summary>
        /// Indicates a specific bus to use
        /// </summary>
        /// <typeparam name="T">Type of bus to use.</typeparam>
        /// <returns>Current configuration.</returns>
        public IBusConfiguration UseBus<T>() where T : class, IDomainEventBus
        {
            SetupCurrentConfig();
            _currentConfig.BusTypes = new[] { typeof(T) };
            return this;
        }
        
        /// <summary>
        /// Indicates to use all buses available within the system.
        /// </summary>
        /// <returns>Current configuration.</returns>
        public IBusConfiguration UseAllAvailableBuses()
        {
            SetupCurrentConfig();
            _currentConfig.BusTypes = ReflectionTools.GetAllTypes().Where(t => typeof(IDomainEventBus).IsAssignableFrom(t) && t.GetTypeInfo().IsClass)
                .ToArray();
            return this;
        }
        
        /// <summary>
        /// Indicates to uses specified buses passed as parameter.
        /// </summary>
        /// <param name="types">Buses types to use.</param>
        /// <returns>Current configuration.</returns>
        public IBusConfiguration UseBuses(params Type[] types)
        {
            SetupCurrentConfig();
            _currentConfig.BusTypes = types;
            return this;
        }
        
        /// <summary>
        /// Create an error handle to fire if any exception happens on dispatch;
        /// </summary>
        /// <param name="handler">Handler to fire if exception..</param>
        /// <returns>Current configuration.</returns>
        public IBusConfiguration HandleErrorWith(Action<Exception> handler)
        {
            _currentConfig.ErrorHandler = handler;
            return this;
        }
        
        /// <summary>
        /// Specify the serializer for event transport.
        /// </summary>
        /// <typeparam name="T">Type of serializer to use.</typeparam>
        /// <returns>Current configuration.</returns>
        public IBusConfiguration SerializeWith<T>() where T : class, IEventSerializer
        {
            _currentConfig.SerializerType = typeof(T);
            return this;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Setting up the current config.
        /// </summary>
        private void SetupCurrentConfig()
        {
            if (_currentConfig == null)
            {
                _currentConfig = new EventDispatchConfigurationBuilder();
                _busConfigs.Add(_currentConfig);
            }
            else
            {
                var cfg = new EventDispatchConfigurationBuilder();
                _busConfigs.Add(cfg);
                _currentConfig = cfg;
            }
        }


        #endregion

    }
}
