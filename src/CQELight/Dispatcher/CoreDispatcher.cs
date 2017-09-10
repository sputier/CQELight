using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher.Configuration;
using CQELight.IoC;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Dispatcher
{
    /// <summary>
    /// Dispatcher of events and commands.
    /// </summary>
    public static class CoreDispatcher
    {

        #region Events

        /// <summary>
        /// Callback method to invoke when an event is dispatched.
        /// </summary>
        internal static event Action<IDomainEvent> OnEventDispatched;

        /// <summary>
        /// Callback method to invoke when a command is dispatched.
        /// </summary>
        internal static event Action<ICommand> OnCommandDispatched;

        /// <summary>
        /// IoC scope for the dispatcher.
        /// </summary>
        internal readonly static IScope _scope;

        #endregion

        #region Static members

        /// <summary>
        /// Dispatcher configuration
        /// </summary>
        static CoreDispatcherConfiguration _config;

        /// <summary>
        /// Flag that indicates if the dispatcher is already configured.
        /// </summary>
        static bool _isConfigured;

        #endregion

        #region Static accessor

        /// <summary>
        /// Static accessor.
        /// </summary>
        static CoreDispatcher()
        {
            _scope = DIManager.BeginScope();
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Defines the configuration to use.
        /// </summary>
        /// <param name="config">Configuration to use.</param>
        public static void UseConfiguration(CoreDispatcherConfiguration config)
        {
            _config = config;
            _isConfigured = true;
        }

        /// <summary>
        /// Dispatch asynchronously an event and its context within every bus that it's configured for.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        /// <param name="context">Context to associate.</param>
        public static async Task DispatchEventAsync(IDomainEvent @event, IEventContext context = null)
        {
            if (!_isConfigured)
                UseConfiguration(CoreDispatcherConfiguration.Default);
            if (OnEventDispatched != null)
            {
                foreach (Action<IDomainEvent> act in OnEventDispatched.GetInvocationList())
                {
                    act(@event);
                }
            }
            var eventType = @event.GetType();
            foreach (var dispatcher in _config.EventDispatchersConfiguration
                .Where(e => EventTypeMatch(e.Key, eventType))
                .SelectMany(m => m.Value))
            {
                try
                {
                    await dispatcher.Bus.RegisterAsync(@event, context);
                }
                catch (Exception e)
                {
                    dispatcher.ErrorHandler(e);
                }
            }
        }

        /// <summary>
        /// Dispatch synchronously an event and its context within every bus that it's configured for.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        /// <param name="context">Context to associate.</param>
        public static void DispatchEvent(IDomainEvent @event, IEventContext context = null)
        {
            if (!_isConfigured)
                UseConfiguration(CoreDispatcherConfiguration.Default);
            DispatchEventAsync(@event, context).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Dispatch asynchronously a command and its context within every bus that it's configured for.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="context">Context to associate.</param>
        /// <returns>Awaiter of events.</returns>
        public static async Task<DispatcherAwaiter> DispatchCommandAsync(ICommand command, ICommandContext context = null)
        {
            if (!_isConfigured)
                UseConfiguration(CoreDispatcherConfiguration.Default);
            if (OnCommandDispatched != null)
            {
                foreach (Action<ICommand> act in OnCommandDispatched.GetInvocationList())
                {
                    act(command);
                }
            }
            if (DIManager.IsInit)
            {

                var types = ReflectionTools.GetAllTypes().Where(t => t.GetTypeInfo().IsClass
                    && typeof(ICommandBus).GetTypeInfo().IsAssignableFrom(t)).ToList();
                foreach (var dispatcher in types)
                {
                    await ((ICommandBus)_scope.Resolve(dispatcher)).DispatchAsync(command, context);
                }
            }
            return new DispatcherAwaiter();
        }

        /// <summary>
        /// Dispatch synchronously a command and its context within every bus that it's configured for.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="context">Context to associate.</param>
        /// <returns>Awaiter of events.</returns>
        public static DispatcherAwaiter DispatchCommand(ICommand command, ICommandContext context = null)
        {
            if (!_isConfigured)
                UseConfiguration(CoreDispatcherConfiguration.Default);
            return DispatchCommandAsync(command, context).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Applies a configuration to a bus, overriding the previous one if any.
        /// </summary>
        /// <typeparam name="T">Type of bus.</typeparam>
        /// <typeparam name="TConfig">Type of config.</typeparam>
        /// <param name="config">Instance of configuration.</param>
        public static void ConfigureBus<T, TConfig>(TConfig config)
            where T : IConfigurableBus<TConfig>
            where TConfig : IDomainEventBusConfiguration
        {
            var busInstance = _scope.Resolve(typeof(T)) as IConfigurableBus<TConfig>;
            if (busInstance != null)
            {
                busInstance.Configure(config);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Check if two event types are compatible to invoke handler.
        /// </summary>
        /// <param name="eventType">First type of event.</param>
        /// <param name="otherEventType">Second type of event.</param>
        /// <returns>True if match, false otherwise.</returns>
        private static bool EventTypeMatch(Type eventType, Type otherEventType)
            =>
                eventType == otherEventType // Same type
            ||
                (
                eventType.GetTypeInfo().IsGenericType && otherEventType.GetTypeInfo().IsGenericType // Generic event ...
             && eventType.GetTypeInfo().GetGenericTypeDefinition() == otherEventType.GetTypeInfo().GetGenericTypeDefinition() // ... with same generic definition ...
             && eventType.GetTypeInfo().GenericTypeParameters[0].GetTypeInfo() // ... and argument is ...
                    .ImplementedInterfaces.Any(i =>
                        i.IsAssignableFrom(otherEventType.GetTypeInfo().GenericTypeArguments[0]) // ... an implemented interface!
                        ||
                        otherEventType.GetTypeInfo().GenericTypeArguments[0]
                            .IsInHierarchySubClassOf(eventType.GetTypeInfo().GenericTypeParameters[0].GetTypeInfo().BaseType))  // ... a class sub-type!
                )
            ;

        #endregion

    }
}
