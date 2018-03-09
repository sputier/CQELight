using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher.Configuration;
using CQELight.IoC;
using CQELight.Tools.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.Dispatcher
{
    /// <summary>
    /// Dispatcher of events and commands.
    /// </summary>
    public static class CoreDispatcher
    {

        #region Events

#pragma warning disable S3264 // Events should be invoked
        /// <summary>
        /// Callback method to invoke when an event is dispatched.
        /// </summary>
        internal static event Func<IDomainEvent, Task> OnEventDispatched;
#pragma warning restore S3264 // Events should be invoked

#pragma warning disable S3264 // Events should be invoked
        /// <summary>
        /// Callback method to invoke when a command is dispatched.
        /// </summary>
        internal static event Action<ICommand> OnCommandDispatched;
#pragma warning restore S3264 // Events should be invoked

        #endregion

        #region Static members

        /// <summary>
        /// Current dispatcher scope.
        /// </summary>
        private static IScope _dispatcherScope;

        /// <summary>
        /// Dispatcher configuration
        /// </summary>
        static CoreDispatcherConfiguration _config;

        /// <summary>
        /// Flag that indicates if the dispatcher is already configured.
        /// </summary>
        static bool _isConfigured;

        /// <summary>
        /// IoC scope for the dispatcher.
        /// </summary>
        internal readonly static IScope _scope;
        /// <summary>
        /// Logger.
        /// </summary>
        static readonly ILogger _logger;
        /// <summary>
        /// Thread safety for static handlers of CoreDispatcher.
        /// </summary>
        static SemaphoreSlim s_HandlerManagementLock = new SemaphoreSlim(1);
        /// <summary>
        /// Collection of events handlers staticly added to CoreDispatcher.
        /// </summary>
        static ConcurrentBag<WeakReference<object>> s_EventHandlers = new ConcurrentBag<WeakReference<object>>();
        /// <summary>
        /// Collection of command handlers staticly added to CoreDispatcher.
        /// </summary>
        static ConcurrentBag<WeakReference<object>> s_CommandHandlers = new ConcurrentBag<WeakReference<object>>();
        /// <summary>
        /// Collection of app event handlers staticly added to CoreDispatcher.
        /// </summary>
        static ConcurrentBag<WeakReference<object>> s_AppEventsHandlers = new ConcurrentBag<WeakReference<object>>();

        #endregion

        #region Static accessor

        /// <summary>
        /// Static accessor.
        /// </summary>
        static CoreDispatcher()
        {
            _scope = DIManager.BeginScope();
            _logger = _scope.Resolve<ILoggerFactory>().CreateLogger(nameof(CoreDispatcher));
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Remove a handler instance from CoreDispatcher.
        /// </summary>
        /// <param name="handler">Instance to delete.</param>
        public static void RemoveHandlerFromDispatcher(object handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            var handlerType = handler.GetType();
            var isEventHandler = handlerType.ImplementsRawGenericInterface(typeof(IDomainEventHandler<>));
            var isCommandHandler = handlerType.ImplementsRawGenericInterface(typeof(ICommandHandler<>));
            if (!isEventHandler && !isCommandHandler)
            {
                return;
            }
            s_HandlerManagementLock.Wait();
            try
            {
                if (isCommandHandler)
                {
                    var handlerReference = s_CommandHandlers.FirstOrDefault(c =>
                    {
                        if (c.TryGetTarget(out object h))
                            return h == handler;
                        return false;
                    });
                    if (handlerReference != null)
                    {
                        _logger.LogInformation($"Dispatcher : Remove a command handler of type {handler.GetType()} from CoreDispatcher.");
                        s_CommandHandlers = new ConcurrentBag<WeakReference<object>>(s_CommandHandlers.Except(new[] { handlerReference }));
                    }
                }
                if (isEventHandler)
                {
                    var handlerReference = s_EventHandlers.FirstOrDefault(c =>
                    {
                        if (c.TryGetTarget(out object h))
                            return h == handler;
                        return false;
                    });
                    if (handlerReference != null)
                    {
                        _logger.LogInformation($"Dispatcher : Remove an event handler of type {handler.GetType()} from CoreDispatcher.");
                        s_EventHandlers = new ConcurrentBag<WeakReference<object>>(s_EventHandlers.Except(new[] { handlerReference }));
                    }
                }
                LogThreadInfos();
            }
            finally
            {
                s_HandlerManagementLock.Release();
            }
        }

        /// <summary>
        /// Ajout d'un handler d'un commande ou d'un domainEvent dans le Dispatcher pour utilisation InMemory.
        /// </summary>
        /// <param name="handler">Instance d'handler.</param>
        public static void AddHandlerToDispatcher(object handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            var handlerType = handler.GetType();
            var isEventHandler = handlerType.ImplementsRawGenericInterface(typeof(IDomainEventHandler<>));
            var isCommandHandler = handlerType.ImplementsRawGenericInterface(typeof(ICommandHandler<>));
            if (!isEventHandler && !isCommandHandler)
            {
                return;
            }
            s_HandlerManagementLock.Wait();
            try
            {
                if (isCommandHandler && !s_CommandHandlers.Any(c =>
                      {
                          c.TryGetTarget(out object h);
                          return h == handler;
                      }))
                {
                    _logger.LogInformation($"Dispatcher : Adding a command handler of type {handler.GetType()} in CoreDispatcher.");
                    s_CommandHandlers.Add(new WeakReference<object>(handler));
                }
                if (isEventHandler && !s_EventHandlers.Any(c =>
                      {
                          c.TryGetTarget(out object h);
                          return h == handler;
                      }))
                {
                    _logger.LogInformation($"Dispatcher : Adding an event handler of type {handler.GetType()} in CoreDispatcher.");
                    s_EventHandlers.Add(new WeakReference<object>(handler));
                }
                LogThreadInfos();
            }
            finally
            {
                s_HandlerManagementLock.Release();
            }
        }

        /// <summary>
        /// Dispatch asynchronously an event and its context within every bus that it's configured for.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        /// <param name="context">Context to associate.</param>
        /// <param name="callerMemberName">Caller name.</param>
        public static async Task DispatchEventAsync(IDomainEvent @event, IEventContext context = null, [CallerMemberName] string callerMemberName = "")
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }
            var eventType = @event.GetType();
            _logger.LogInformation($"Dispatcher : Beginning of dispatch event of type {eventType.FullName} from {callerMemberName}");
            _logger.LogInformation($"Dispatcher : Type of context associated to event {eventType.FullName} : {(context == null ? "none" : context.GetType().FullName)}");

            try
            {
#pragma warning disable CS4014
                Task.Run(() => _logger.LogDebug($"Dispatcher : Event data : {Environment.NewLine}{@event.ToJson()}"));
#pragma warning restore
            }
            catch
            {
                //Useless for logging purpose.
            }

            LogThreadInfos();
            if (!_isConfigured)
            {
                UseConfiguration(CoreDispatcherConfiguration.Default);
            }
            if (OnEventDispatched != null)
            {
                foreach (Func<IDomainEvent, Task> act in OnEventDispatched.GetInvocationList().OfType<Func<IDomainEvent, Task>>())
                {
                    try
                    {
                        _logger.LogInformation($"Dispatcher : Invoke of action {act.Method.Name} on" +
                            $" {(act.Target != null ? act.Target.GetType().FullName : act.Method.DeclaringType.FullName)} for event {eventType.FullName}");
                        await act(@event);
                    }
                    catch (Exception e)
                    {
                        _logger.LogErrorMultilines($"CoreDispatcher.DispatchEventAsync() : Cannot call" +
                            $" action {act.Method.Name} one {(act.Target != null ? act.Target.GetType().FullName : act.Method.DeclaringType.FullName)}" +
                            $" for event {eventType.FullName}",
                            e.ToString());
                    }
                }
            }
            foreach (var dispatcher in _config.EventDispatchersConfiguration
                .Where(e => EventTypeMatch(e.Key, eventType))
                .SelectMany(m => m.Value).WhereNotNull())
            {
                try
                {
                    if (GetScope().Resolve(dispatcher.BusType) is IDomainEventBus busInstance)
                    {
                        _logger.LogInformation($"Dispatcher : Sending the event {eventType.FullName} on bus {dispatcher.BusType}");
                        await busInstance.RegisterAsync(@event, context);
                    }
                    else
                    {
                        _logger.LogWarning($"Dispatcher : Instance of events bus {dispatcher.BusType.FullName} cannot be retrieved from scope.");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogErrorMultilines($"CoreDispatcher.DispatchEventAsync() : Exception when sending event {eventType.FullName} on bus {dispatcher.BusType}",
                        e.ToString());
                    dispatcher.ErrorHandler?.Invoke(e);
                }
            }
            _logger.LogInformation($"Dispatcher : End of sending event of type {eventType.FullName}");
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
        /// <param name="callerMemberName">Calling method.</param>
        /// <returns>Awaiter of events.</returns>
        public static async Task<DispatcherAwaiter> DispatchCommandAsync(ICommand command, ICommandContext context = null, [CallerMemberName] string callerMemberName = "")
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }
            var commandType = command.GetType();
            _logger.LogInformation($"Dispatcher : Beginning of sending command of type {commandType.FullName} from {callerMemberName}");
            _logger.LogInformation($"Dispatcher : Type of context associated with command {commandType.FullName} : {(context == null ? "none" : context.GetType().FullName)}");
            try
            {
#pragma warning disable CS4014
                Task.Run(() => _logger.LogDebug($"Dispatcher : Command's data : {command.ToJson()}"));
#pragma warning restore
            }
            catch
            {
                //No need to throw exception for logging purpose.
            }

            LogThreadInfos();

            if (!_isConfigured)
                UseConfiguration(CoreDispatcherConfiguration.Default);
            if (OnCommandDispatched != null)
            {
                foreach (Func<ICommand, Task> act in OnCommandDispatched.GetInvocationList().OfType<Func<ICommand, Task>>())
                {
                    _logger.LogInformation($"Dispatcher : Invoke action {act.Method.Name} on {(act.Target != null ? act.Target.GetType().FullName : act.Method.DeclaringType.FullName)} " +
                        $"for command {commandType.FullName}");
                    try
                    {
                        await act(command);
                    }
                    catch (Exception e)
                    {
                        _logger.LogErrorMultilines($"CoreDispatcher.DispatchCommandAsync() : " +
                            $"Cannot call action {act.Method.Name} on {(act.Target != null ? act.Target.GetType().FullName : act.Method.DeclaringType.FullName)} for command {commandType.FullName}",
                            e.ToString());
                    }
                }
            }
            var tasks = new List<Task>();
            var awaiter = new DispatcherAwaiter(tasks);

            if (DIManager.IsInit)
            {
                var buses = GetScope().ResolveAllInstancesOf<ICommandBus>();
                foreach (var bus in buses)
                {
                    _logger.LogInformation($"Dispatcher : Sending command {commandType.FullName} on bus {bus.GetType().FullName}");
                    tasks.AddRange(await bus.DispatchAsync(command, context));
                }
            }
            return awaiter;
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

        /// <summary>
        /// Try to get all event handlers staticly added for a specific event type.
        /// </summary>
        /// <typeparam name="T">Type of event to search handlers for.</typeparam>
        /// <returns>Collection of event handlers.</returns>
        public static IEnumerable<object> TryGetHandlersForEvent<T>()
            where T : IDomainEvent
            => TryGetHandlersForEventType(typeof(T));
        
        /// <summary>
        /// Try to get all event handlers staticly added for a specific event type.
        /// </summary>
        /// <param name="type">Type of event to search handlers for.</param>
        /// <returns>Collection of event handlers.</returns>
        public static IEnumerable<object> TryGetHandlersForEventType(Type type)
        {
            s_HandlerManagementLock.Wait();
            try
            {
                var results = new List<object>();
                var toDelete = new List<WeakReference<object>>();
                foreach (var handler in s_EventHandlers)
                {
                    if (handler.TryGetTarget(out object instance))
                    {
                        if (instance.GetType().GetInterfaces()
                            .Any(i => i.IsGenericType
                                   && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
                                   && i.GenericTypeArguments[0] == type)
                            && !results.Any(r => r == instance))
                        {
                            results.Add(instance);
                        }
                    }
                    else
                    {
                        toDelete.Add(handler);
                    }
                }

                if (toDelete.Any())
                {
                    var copy = s_EventHandlers.ToList();
                    s_EventHandlers = new ConcurrentBag<WeakReference<object>>();
                    foreach (var item in copy)
                    {
                        if (!toDelete.Any(d => d == item))
                        {
                            s_EventHandlers.Add(item);
                        }
                    }
                }
                return results;
            }
            finally
            {
                s_HandlerManagementLock.Release();
            }
        }

        /// <summary>
        /// Try to get event handler staticly added for a specific command type.
        /// </summary>
        /// <typeparam name="T">Type of command to search handlers for.</typeparam>
        /// <returns>Instance of event handler.</returns>
        public static object TryGetHandlerForCommand<T>()
            where T : ICommand
         => TryGetHandlerForCommandType(typeof(T));

        /// <summary>
        /// Try to get event handler staticly added for a specific command type.
        /// </summary>
        /// <param name="type">Type of event to search handlers for.</param>
        /// <returns>Instance of event handler.</returns>
        public static object TryGetHandlerForCommandType(Type type)
        {
            s_HandlerManagementLock.Wait();
            try
            {
                object result = null;
                var toDelete = new List<WeakReference<object>>();
                foreach (var handler in s_CommandHandlers)
                {
                    if (handler.TryGetTarget(out object instance))
                    {
                        if (instance.GetType().GetInterfaces()
                            .Any(i => i.IsGenericType
                                   && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                                   && i.GenericTypeArguments[0] == type))
                        {
                            result = instance;
                            break;
                        }
                    }
                    else
                    {
                        toDelete.Add(handler);
                    }
                }

                if (toDelete.Any())
                {
                    var copy = s_CommandHandlers.ToList();
                    s_CommandHandlers = new ConcurrentBag<WeakReference<object>>();
                    foreach (var item in copy)
                    {
                        if (!toDelete.Any(d => d == item))
                        {
                            s_CommandHandlers.Add(item);
                        }
                    }
                }
                return result;
            }
            finally
            {
                s_HandlerManagementLock.Release();
            }
        }

        #endregion

        #region Internal static methods

        /// <summary>
        /// Getting back dispatcher scope, and instantiate a new one if needed.
        /// </summary>
        /// <returns>Scope instance.</returns>
        internal static IScope GetScope(IScope newScope = null)
        {
            if (_dispatcherScope == null || _dispatcherScope.IsDisposed)
            {
                _dispatcherScope = DIManager.BeginScope();
            }
            return _dispatcherScope;
        }

        /// <summary>
        /// Defines the configuration to use.
        /// </summary>
        /// <param name="config">Configuration to use.</param>
        internal static void UseConfiguration(CoreDispatcherConfiguration config)
        {
            _config = config;
            _isConfigured = true;
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

        /// <summary>
        /// Add some thread infos as logging data.
        /// </summary>
        private static void LogThreadInfos()
        {
            try
            {
                _logger.LogDebug($"Thread infos :{Environment.NewLine}");
                _logger.LogDebug($"id = {Thread.CurrentThread.ManagedThreadId}{Environment.NewLine}");
                _logger.LogDebug($"priority = {Thread.CurrentThread.Priority}{Environment.NewLine}");
                _logger.LogDebug($"name = {Thread.CurrentThread.Name}{Environment.NewLine}");
                _logger.LogDebug($"state = {Thread.CurrentThread.ThreadState}{Environment.NewLine}");
                _logger.LogDebug($"culture = {Thread.CurrentThread.CurrentCulture?.Name}{Environment.NewLine}");
                _logger.LogDebug($"ui culture = {Thread.CurrentThread.CurrentUICulture?.Name}{Environment.NewLine}");
            }
            catch
            {
                //No need to stop working for logging
            }
        }

        #endregion

    }
}
