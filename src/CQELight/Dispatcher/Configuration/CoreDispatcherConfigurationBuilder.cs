using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Dispatcher;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher.Configuration.Commands;
using CQELight.Dispatcher.Configuration.Events;
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

        private readonly ICollection<SingleEventTypeConfiguration> _singleEventConfigs;
        private readonly ICollection<MultipleEventTypeConfiguration> _multipleEventConfigs;

        private readonly ICollection<SingleCommandTypeConfiguration> _singleCommandConfigs;
        private readonly ICollection<MultipleCommandTypeConfiguration> _multipleCommandConfigs;

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
            _singleCommandConfigs = new List<SingleCommandTypeConfiguration>();
            _multipleCommandConfigs = new List<MultipleCommandTypeConfiguration>();

            if (scopeFactory != null)
            {
                _scope = scopeFactory.CreateScope();
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets a configuration to apply to all commands of the app.
        /// </summary>
        /// <returns>Mutilple command type configuration</returns>
        public MultipleCommandTypeConfiguration ForAllCommands()
            => ForCommands(ReflectionTools.GetAllTypes()
                   .Where(t => typeof(ICommand).GetTypeInfo().IsAssignableFrom(t) && t.GetTypeInfo().IsClass).ToArray());


        /// <summary>
        /// Gets a configuration to apply to all commands that were not configured yet.
        /// </summary>
        /// <returns>Mutilple command type configuration</returns>
        public MultipleCommandTypeConfiguration ForAllOtherCommands()
        {
            var eventTypes = ReflectionTools.GetAllTypes()
                .Where(t => typeof(ICommand).GetTypeInfo().IsAssignableFrom(t) && t.GetTypeInfo().IsClass
                            && !_singleCommandConfigs.Any(c => c._commandType == t)
                            && !_multipleCommandConfigs.Any(m => m._commandTypesConfigs.Any(c => c._commandType == (t))));
            if (eventTypes.Any())
            {
                return ForCommands(eventTypes.ToArray());
            }
            return new MultipleCommandTypeConfiguration();
        }

        /// <summary>
        /// Gets a configuration to apply to all commands types passed as parameters.
        /// </summary>
        /// <param name="commandTypes">Types of commands to configure.</param>
        /// <returns>Mutilple command type configuration</returns>
        public MultipleCommandTypeConfiguration ForCommands(params Type[] commandTypes)
        {
            var config = new MultipleCommandTypeConfiguration(commandTypes.ToArray());
            _multipleCommandConfigs.Add(config);
            return config;
        }

        /// <summary>
        /// Gets a configuration to apply to a single command type.
        /// </summary>
        /// <typeparam name="T">Command's type.</typeparam>
        /// <returns>Single command type configuration.</returns>
        public SingleCommandTypeConfiguration ForCommand<T>()
            where T : ICommand
        {
            var config = new SingleCommandTypeConfiguration(typeof(T));
            _singleCommandConfigs.Add(config);
            return config;
        }


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
        public DispatcherConfiguration Build(bool strict = false)
        {
            if (_singleEventConfigs.Count > 0 || _multipleEventConfigs.Count > 0 
             || _singleCommandConfigs.Count > 0  || _multipleCommandConfigs.Count > 0)
            {
                var config = new DispatcherConfiguration(strict);
                config.EventDispatchersConfiguration =
                    _singleEventConfigs.Concat(_multipleEventConfigs.SelectMany(m => m._eventTypesConfigs))
                    .Select(e => new EventDispatchConfiguration
                    {
                        EventType = e._eventType,
                        ErrorHandler = e._errorHandler,
                        Serializer = e._serializerType != null ? GetSerializer(e._serializerType) : null,
                        IsSecurityCritical = e._isSecurityCritical,
                        BusesTypes = e._busConfigs
                    });
                config.CommandDispatchersConfiguration =
                    _singleCommandConfigs.Concat(_multipleCommandConfigs.SelectMany(m => m._commandTypesConfigs))
                    .Select(e => new CommandDispatchConfiguration
                    {
                        CommandType = e._commandType,
                        ErrorHandler = e._errorHandler,
                        Serializer = e._serializerType != null ? GetSerializer(e._serializerType) : null,
                        IsSecurityCritical = e._isSecurityCritical,
                        BusesTypes = e._busConfigs
                    });
                return config;
            }
            return DispatcherConfiguration.Default;
        }

        #endregion

        #region Private methods

        private IDispatcherSerializer GetSerializer(Type serializerType)
            => (_scope?.Resolve(serializerType) ?? serializerType.CreateInstance()) as IDispatcherSerializer;

        #endregion

    }
}
