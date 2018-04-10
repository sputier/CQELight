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
using System.Linq.Expressions;
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
    public sealed class InMemoryEventBus : IDomainEventBus
    {

        #region Private static members

        private static IEnumerable<Type> s_eventHandlers;

        #endregion

        #region Private members

        private readonly Dictionary<Type, MethodInfo> _handlers_HandleMethods;
        private InMemoryEventBusConfiguration _config = InMemoryEventBusConfiguration.Default;
        private readonly IScope _scope;
        private readonly ILogger _logger;


        #endregion

        #region Ctor

        static InMemoryEventBus()
        {
            s_eventHandlers = ReflectionTools.GetAllTypes().Where(IsEventHandler).ToList();
        }

        internal InMemoryEventBus(InMemoryEventBusConfiguration configuration = null, IScopeFactory scopeFactory = null)
        {
            if (scopeFactory != null)
            {
                _scope = scopeFactory.CreateScope();
            }
            _logger =
                _scope?.Resolve<ILoggerFactory>()?.CreateLogger<InMemoryEventBus>()
                ??
                new LoggerFactory().CreateLogger<InMemoryEventBus>();
            _handlers_HandleMethods = new Dictionary<Type, MethodInfo>();
            _config = configuration ?? InMemoryEventBusConfiguration.Default;
        }

        #endregion

        #region Private static methods

        private static bool IsEventHandler(Type x)
            => x.GetInterfaces().Any(y => y.GetTypeInfo().IsGenericType
                                       && (y.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>) || y.GetGenericTypeDefinition() == typeof(ITransactionnalEventHandler<>)));

        #endregion

        #region Private methods

        private IEnumerable<(Type HandlerType, byte Priority)> GetHandlerForEventType(Type eventType)
        => s_eventHandlers.Where(handlerType => HandlerTypeCompatibleWithEvent(eventType, handlerType))
                .Select(handlerType =>
                {
                    byte priority = handlerType.GetCustomAttribute<DispatcherPriorityAttribute>()?.Priority ?? 0;
                    return (handlerType, priority);
                });

        private bool HandlerTypeCompatibleWithEvent(Type eventType, Type handlerType)
         => handlerType?.GetInterfaces().Any(i =>
                            i.GetTypeInfo().IsGenericType
                         && (i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>) || i.GetGenericTypeDefinition() == typeof(ITransactionnalEventHandler<>))
                         && i.GenericTypeArguments[0] == eventType) == true;

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
                        if (context is ISaga saga && saga.Completed)
                        {
                            result = context;
                        }
                        else
                        {
                            _logger.LogWarning($"InMemoryEventBus : Trying to get a saga whereas it's already completed.");
                        }
                    }
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
                    else if (_scope != null)
                    {
                        _logger.LogDebug($"InMemoryEventBus : Getting a handler of type {handlerType.FullName} from general scope.");
                        result = _scope.Resolve(handlerType);
                    }
                    else
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

        private IEnumerable<Type> GetOrderedHandlers(List<(Type HandlerType, byte Priority)> handlers,
            IEnumerable<Type> dispatcherHandlerTypes)
            => handlers.OrderByDescending(h => h.Priority)
                       .Select(h => h.HandlerType)
                       .Where(t => !dispatcherHandlerTypes.Any(i => i == t));

        private Queue<(MethodInfo Method, object Handler)> GetMethods(IDomainEvent @event, IEventContext context)
        {
            var methods = new Queue<(MethodInfo Method, object Handler)>();
            var handlerTypes = new List<Type>();
            var evtType = @event.GetType();

            var handlers = GetHandlerForEventType(evtType).ToList();
            var dispatcherHandlerInstances = CoreDispatcher.TryGetHandlersForEventType(evtType);


            bool hasAtLeastOneActiveHandler = handlers.Count > 0 || dispatcherHandlerInstances.Any();

            if (hasAtLeastOneActiveHandler)
            {
                foreach (var h in GetOrderedHandlers(handlers, dispatcherHandlerInstances.Select(i => i?.GetType())))
                {
                    var handlerInstance = GetOrCreateHandler(h, context);
                    if (handlerInstance != null)
                    {
                        _logger.LogDebug($"InMemoryEventBus : Got handler of type {h.Name} for event's type {evtType.Name}");
                        var handleMethod = h.GetTypeInfo().GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync), new[] { evtType, typeof(IEventContext) });
                        _logger.LogInformation($"InMemoryEventBus : Add method HandleAsync of handler {h.FullName} for event's type {evtType.FullName}");
                        methods.Enqueue((handleMethod, handlerInstance));
                        handlerTypes.Add(h);
                    }
                    else
                    {
                        _logger.LogInformation($"InMemoryEventBus : No dynamic handlers of type {h.FullName} found for event of type {evtType.FullName}");
                    }
                }
                foreach (var handlerInstance in dispatcherHandlerInstances)
                {
                    var handlerType = handlerInstance.GetType();
                    if (!handlerTypes.Contains(handlerType))
                    {
                        var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync), new[] { evtType, typeof(IEventContext) });
                        _logger.LogInformation($"InMemoryEventBus : Add HandlerAsync on {handlerType.FullName} obtained from CoreDispatcher for event of type {evtType.FullName}");
                        methods.Enqueue((handleMethod, handlerInstance));
                    }
                }
            }
            else
            {
                _logger.LogInformation($"InMemoryEventBus : No in memory handlers found for event of type {evtType.Name}.");
            }
            return methods;
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

                var ifClause = _config.IfClauses.FirstOrDefault(i => new TypeEqualityComparer().Equals(i.Key, @event.GetType()));
                if (ifClause.Value?.Invoke(@event) == false)
                {
                    _logger.LogInformation($"InMemoryEventBus : Dispatch clause for event of type {evtType.FullName} prevents dispatching in memory.");
                    return;
                }

                var methods = GetMethods(@event, context);

                var handled = new List<Type>();
                int currentRetry = 0;
                bool allowParallelDispatch = _config.ParallelDispatch.Any(t => new TypeEqualityComparer().Equals(t, evtType));
                do
                {
                    var tasks = new List<(Type HandlerType, Task Task)>();
                    foreach (var (Method, Handler) in methods.Where(t => !handled.Any(h => new TypeEqualityComparer().Equals(h, t.Handler.GetType()))))
                    {
                        try
                        {
                            if (allowParallelDispatch)
                            {
                                tasks.Add((Handler.GetType(), (Task)Method.Invoke(Handler, new object[] { @event, context })));
                            }
                            else
                            {
                                await ((Task)Method.Invoke(Handler, new object[] { @event, context })).ConfigureAwait(false);
                                handled.Add(Handler.GetType());
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogErrorMultilines("InMemoryEventBus.TreatEventsAsync : " +
                                $"{currentRetry}/{_config.NbRetries}) Failed to call HandleAsync on handler of type {Handler.GetType().FullName} " +
                                $"for event's type {evtType.FullName}", e.ToString());
                            currentRetry++;
                            await Task.Delay((int)_config.WaitingTimeMilliseconds).ConfigureAwait(false);
                        }
                    }
                    if (allowParallelDispatch)
                    {
                        var task = Task.WhenAll(tasks.Select(t => t.Task));
                        try
                        {
                            await task.ConfigureAwait(false);
                            break;
                        }
                        catch (Exception e)
                        {
                            _logger.LogErrorMultilines("InMemoryEventBus.TreatEventsAsync : " +
                                $"{currentRetry}/{_config.NbRetries}) Failed to call HandleAsync of an handler in parallel" +
                                $"for event's type {evtType.FullName}", e.ToString());
                            handled.AddRange(tasks.Where(t => t.Task.Status != TaskStatus.Faulted).Select(t => t.HandlerType));
                            currentRetry++;
                            await Task.Delay((int)_config.WaitingTimeMilliseconds).ConfigureAwait(false);
                        }
                    }
                }
                while (handled.Count != methods.Count && _config.NbRetries != 0 && currentRetry < _config.NbRetries);
                if (_config.NbRetries != 0 && currentRetry >= _config.NbRetries)
                {
                    _config.OnFailedDelivery?.Invoke(@event, context);
                    _logger.LogDebug($"InMemoryEventBus : Cannot retrieve an handler in memory for event of type {evtType.Name}.");
                }
            }
        }

        #endregion

    }
}
