using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher.Configuration.Internal;
using CQELight.IoC;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQELight.Dispatcher.Configuration
{
    /// <summary>
    /// Building class for creating a configuration for dispatcher.
    /// </summary>
    public class CoreDispatcherConfigurationBuilder
    {

        #region Members

        /// <summary>
        /// Configurations that matches a single event type.
        /// </summary>
        private readonly ICollection<SingleEventTypeConfiguration> _singleEventConfigs;
        /// <summary>
        /// Configurations that matches multiple event types.
        /// </summary>
        private readonly ICollection<MultipleEventTypeConfiguration> _multipleEventConfigs;
        /// <summary>
        /// Current scope.
        /// </summary>
        internal IScope _scope;

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new builder for building configuration.
        /// </summary>
        public CoreDispatcherConfigurationBuilder(IScopeFactory scopeFactory = null)
        {
            _singleEventConfigs = new List<SingleEventTypeConfiguration>();
            _multipleEventConfigs = new List<MultipleEventTypeConfiguration>();
            if (scopeFactory != null)
            {
                _scope = scopeFactory.CreateScope();
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets a configuration to apply to all events of the app.
        /// </summary>
        /// <returns>Mutilple event type configuration</returns>
        public MultipleEventTypeConfiguration ForAllEvents()
            => ForEvents(ReflectionTools.GetAllTypes()
                   .Where(t => typeof(IDomainEvent).GetTypeInfo().IsAssignableFrom(t) && t.GetTypeInfo().IsClass).ToArray());


        /// <summary>
        /// Gets a configuration to apply to all events that were not configured yet.
        /// </summary>
        /// <returns>Mutilple event type configuration</returns>
        public MultipleEventTypeConfiguration ForAllOtherEvents()
        {
            var eventTypes = ReflectionTools.GetAllTypes()
                .Where(t => typeof(IDomainEvent).GetTypeInfo().IsAssignableFrom(t) && t.GetTypeInfo().IsClass
                            && !_singleEventConfigs.Any(c => c._eventType == t)
                            && !_multipleEventConfigs.Any(m => m._eventTypesConfigs.Any(c => c._eventType == (t))));
            if (eventTypes.Any())
            {
                return ForEvents(eventTypes.ToArray());
            }
            return new MultipleEventTypeConfiguration();
        }

        /// <summary>
        /// Gets a configuration to apply to all events types passed as parameters.
        /// </summary>
        /// <param name="eventTypes">Types of event to configure.</param>
        /// <returns>Mutilple event type configuration</returns>
        public MultipleEventTypeConfiguration ForEvents(params Type[] eventTypes)
        {
            var config = new MultipleEventTypeConfiguration(eventTypes.ToArray());
            _multipleEventConfigs.Add(config);
            return config;
        }

        /// <summary>
        /// Gets a configuration to apply to a single event type.
        /// </summary>
        /// <typeparam name="T">Event's type.</typeparam>
        /// <returns>Single event type configuration.</returns>
        public SingleEventTypeConfiguration ForEvent<T>()
            where T : IDomainEvent
        {
            var config = new SingleEventTypeConfiguration(typeof(T));
            _singleEventConfigs.Add(config);
            return config;
        }

        /// <summary>
        /// Build all the pre-defined configurations within a single configuration object used to configre Dispatcher.
        /// </summary>
        /// <param name="strict">Set the strict flag on the configuration</param>
        /// <returns>Dispatcher's configuration.</returns>
        public CoreDispatcherConfiguration Build(bool strict = false)
        {
            if (_singleEventConfigs.Any() || _multipleEventConfigs.Any())
            {
                var config = new CoreDispatcherConfiguration(strict);
                config.EventDispatchersConfiguration =
                    _singleEventConfigs.Concat(_multipleEventConfigs.SelectMany(m => m._eventTypesConfigs))
                    .Select(e => new EventDispatchConfiguration
                    {
                        EventType = e._eventType,
                        ErrorHandler = e._errorHandler,
                        Serializer = GetSerializer(e._serializerType),
                        IsSecurityCritical = e._isSecurityCritical,
                        BusesTypes = e._busConfigs
                    });
                return config;
            }
            return CoreDispatcherConfiguration.Default;
        }

        #endregion

        #region Private methods

        private IEventSerializer GetSerializer(Type serializerType)
            => (_scope?.Resolve(serializerType) ?? serializerType.CreateInstance()) as IEventSerializer;

        #endregion

    }
}
