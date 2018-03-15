using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Abstractions.Saga.Interfaces;
using CQELight.Dispatcher;
using CQELight.IoC;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.Buses.InMemory.Events
{
    /// <summary>
    /// InMemory stateless bus to dispatch events.
    /// If program shutdowns unexpectedly, it means all events stored in it are lost and cannot be retrieved. 
    /// However, this is a very fast bus for dispatch.
    /// </summary>
    public sealed class InMemoryEventBus : IDomainEventBus, IConfigurableBus<InMemoryEventBusConfiguration>
    {

        #region Private static members

        private static IEnumerable<Type> s_eventHandlers;
        private static InMemoryEventBusConfiguration _config;

        #endregion

        #region Private members

        private readonly Dictionary<Type, MethodInfo> _handlers_HandleMethods;
        private readonly IScope _scope;
        private readonly ICollection<IEventAwaiter> _eventAwaiters;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Static accessor.
        /// </summary>
        static InMemoryEventBus()
        {
            s_eventHandlers = ReflectionTools.GetAllTypes().Where(IsEventHandler).ToList();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        internal InMemoryEventBus()
        {
            _scope = DIManager.BeginScope();
            _logger = _scope.Resolve<ILoggerFactory>().CreateLogger<InMemoryEventBus>();
            _handlers_HandleMethods = new Dictionary<Type, MethodInfo>();
            _eventAwaiters = new List<IEventAwaiter>();
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Check if the type is a valid event handler.
        /// </summary>
        /// <param name="x">Type to check.</param>
        /// <returns>True if type can be use to handle events, false otherwise.</returns>
        private static bool IsEventHandler(Type x)
            => x.GetInterfaces().Any(y => y.GetTypeInfo().IsGenericType
                                       && y.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));


        #endregion

        #region Private methods

        /// <summary>
        /// Get handlers with their priority for a particular event type.
        /// </summary>
        /// <param name="eventType">Event's type.</param>
        /// <returns>Collection of handlers and their associated priority.</returns>
        private IEnumerable<(Type HandlerType, byte Priority)> GetHandlerForEventType(Type eventType)
        => s_eventHandlers.Where(handlerType => HandlerTypeCompatibleWithEvent(eventType, handlerType))
                .Select(handlerType =>
                {
                    byte priority = handlerType.GetCustomAttribute<DispatcherPriorityAttribute>()?.Priority ?? 0;
                    return (handlerType, priority);
                });

        /// <summary>
        /// Check if an handler type is compatible with a particular event type.
        /// </summary>
        /// <param name="eventType">Event's type to check.</param>
        /// <param name="handlerType">Handler's type to check.</param>
        /// <returns>True if handler's type can handle event's type, false otherwise.</returns>
        private static bool HandlerTypeCompatibleWithEvent(Type eventType, Type handlerType)
         => handlerType != null && handlerType.GetInterfaces().Any(i =>
                            i.GetTypeInfo().IsGenericType
                         && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
                         && i.GenericTypeArguments[0] == eventType);

        /// <summary>
        /// Try to retrieve an handler or create it dynamically.
        /// </summary>
        /// <param name="handlerType">Type of handler..</param>
        /// <param name="context">Context of event.</param>
        /// <returns>Handler instance.</returns>
        private object GetOrCreateHandler(Type handlerType, IEventContext context)
        {
            object result = null;
            try
            {
                if (typeof(ISaga).IsAssignableFrom(handlerType))
                {
                    _logger.LogDebug($"InMemoryEventBus : Getting a handler of type {handlerType.FullName} which is a saga.");
                    if (context != null && context.GetType() == handlerType)
                    {
                        if (!(context as ISaga).Completed)
                        {
                            result = context;
                        }
                        else
                        {
                            _logger.LogWarning($"InMemoryEventBus : Trying to get a saga whereas it's already completed.");
                        }
                    }
                }
                else if (typeof(IEventAwaiter).IsAssignableFrom(handlerType))
                {
                    _logger.LogDebug($"InMemoryEventBus : Getting a handler of type {handlerType.FullName} from bus awaiters.");
                    result = _eventAwaiters.FirstOrDefault(a => a.GetType() == handlerType);
                }
                else if (handlerType == context?.GetType())
                {
                    _logger.LogDebug($"InMemoryEventBus : Getting a handler of type {handlerType.FullName} which is current dispatch context.");
                    result = context;
                }
                else
                {
                    if (context is IScope scope && !scope.IsDisposed)
                    {
                        _logger.LogDebug($"InMemoryEventBus : Getting a handler of type {handlerType.FullName} from the context.");
                        result = scope.Resolve(handlerType);

                    }
                    else if (context is IScopeHolder scopeHolder && !scopeHolder.Scope.IsDisposed)
                    {
                        _logger.LogDebug($"InMemoryEventBus : Getting a handler of type {handlerType.FullName} from the scope of the context.");
                        result = scopeHolder.Scope.Resolve(handlerType);
                    }
                    else
                    {
                        _logger.LogDebug($"InMemoryEventBus : Getting a handler of type {handlerType.FullName} from general scope.");
                        result = _scope.Resolve(handlerType);
                    }
                    if (result == null)
                    {
                        _logger.LogDebug($"InMemoryEventBus : Trying to get handler of type {handlerType.FullName} via reflexion.");
                        result = handlerType.CreateInstance();
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorMultilines($"Cannot retrieve any handler of type {handlerType.FullName}", ex.ToString());
            }
            return result;
        }

        #endregion

        #region IDomainEventBus methods

        /// <summary>
        /// Register asynchronously an event to be processed by the bus.
        /// </summary>
        /// <param name="event">Event to register.</param>
        /// <param name="context">Context associated to the event..</param>
        public async Task RegisterAsync(IDomainEvent @event, IEventContext context = null)
        {
            if (@event != null)
            {
                var evtType = @event.GetType();
                _logger.LogInformation($"InMemoryEventBus : Beginning of treating event's type {evtType.FullName}");
                var handlers = GetHandlerForEventType(evtType).ToList();
                if (context != null)
                {
                    var ctxType = context.GetType();
                    if (HandlerTypeCompatibleWithEvent(evtType, ctxType) && !handlers.Any(h => h.HandlerType == ctxType))
                    {
                        var priority = ctxType.GetCustomAttribute<DispatcherPriorityAttribute>()?.Priority ?? 0;
                        handlers.Add((ctxType, priority));
                        _logger.LogInformation($"InMemoryEventBus : Adding the context as handler for event's type {evtType.FullName}");
                    }
                }
                var handledTypes = new List<Type>();
                bool handledOnce = false;
                int currentRetry = 0;
                var dispatcherHandlerInstances = CoreDispatcher.TryGetHandlersForEventType(evtType);
                bool hasAtLeastOneActiveHandler = handlers.Any() || dispatcherHandlerInstances.Any();
                if (hasAtLeastOneActiveHandler)
                {
                    while (!handledOnce && currentRetry < _config.NbRetries)
                    {
                        if (handlers.Any())
                        {
                            foreach (var h in
                                handlers.OrderByDescending(h => h.Priority)
                                .Select(h => h.HandlerType)
                                .Where(t => !dispatcherHandlerInstances.Any(i => i?.GetType() == t)))
                            {
                                var name = h.Name;
                                var handlerInstance = GetOrCreateHandler(h, context);
                                if (handlerInstance != null)
                                {
                                    _logger.LogDebug($"InMemoryEventBus : Got handler of type {h.Name} for event's type {evtType.Name}");
                                    var handleMethod = h.GetTypeInfo().GetMethod("HandleAsync", new[] { evtType, typeof(IEventContext) });
                                    _logger.LogInformation($"InMemoryEventBus : Calling method HandleAsync of handler {h.FullName} for event's type {evtType.FullName}");
                                    try
                                    {
                                        await (Task)handleMethod.Invoke(handlerInstance, new object[] { @event, context });
                                    }
                                    catch (Exception e)
                                    {
                                        _logger.LogErrorMultilines($"InMemoryEventBus.TreatEventsAsync : {currentRetry}/{_config.NbRetries}) Failed to call HandleAsync on handler of type {h.FullName} for event's type {evtType.FullName}",
                                            e.ToString());
                                    }
                                    handledTypes.Add(h);
                                    handledOnce = true;
                                }
                                else
                                {
                                    _logger.LogInformation($"InMemoryEventBus : No dynamic handlers of type {h.FullName} found for event of type {evtType.FullName}");
                                }
                            }
                        }
                        foreach (var handlerInstance in dispatcherHandlerInstances)
                        {
                            var handlerType = handlerInstance.GetType();
                            if (!handledTypes.Contains(handlerType))
                            {
                                var handleMethod = handlerType.GetMethod("HandleAsync", new[] { evtType, typeof(IEventContext) });
                                _logger.LogInformation($"InMemoryEventBus : Call HandlerAsync on {handlerType.FullName} obtained from CoreDispatcher for event of type {evtType.FullName}");
                                try
                                {
                                    await (Task)handleMethod.Invoke(handlerInstance, new object[] { @event, context });
                                }
                                catch (Exception e)
                                {
                                    _logger.LogErrorMultilines($"InMemoryEventBus : {currentRetry}/{_config.NbRetries}) Fail to call HandleAsync method on {handlerType.FullName} obtained from CoreDispatcher for event {evtType.FullName}",
                                        e.ToString());
                                }
                                handledOnce = true;
                            }
                        }
                        if (!handledOnce)
                        {
                            currentRetry++;
                            await Task.Delay((int)_config.WaitingTimeMilliseconds);
                            if (currentRetry == _config.NbRetries)
                            {
                                _config.OnFailedDelivery?.Invoke(@event, context);
                                _logger.LogDebug($"InMemoryEventBus : Cannot retrieve an handler in memory for event of type {evtType.Name}.");
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"InMemoryEventBus : No in memory handlers found for event of type {evtType.Name}.");
                }
            }
        }

        #endregion

        #region IConfigurable methods

        /// <summary>
        /// Apply passed configuration to the bus.
        /// </summary>
        /// <param name="config">Configuration to use.</param>
        public void Configure(InMemoryEventBusConfiguration config)
            => _config = config;

        #endregion

    }
}
