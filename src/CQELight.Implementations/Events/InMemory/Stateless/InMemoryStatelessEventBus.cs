using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Abstractions.Saga.Interfaces;
using CQELight.Dispatcher;
using CQELight.IoC;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.Implementations.Events.InMemory.Stateless
{
    /// <summary>
    /// InMemory stateless bus to dispatch events.
    /// If program shutdowns unexpectedly, it means all events stored in it are lost and cannot be retrieved. 
    /// However, this is a very fast bus for dispatch.
    /// </summary>
    public class InMemoryStatelessEventBus : IDomainEventBus
    {

        #region Private members

        /// <summary>
        /// Collection of event handlers.
        /// </summary>
        private readonly IEnumerable<Type> _eventHandlers;
        /// <summary>
        /// Current list of events to process.
        /// </summary>
        private readonly Dictionary<IDomainEvent, IEventContext> _events;
        /// <summary>
        /// Cache for methodInfo.
        /// </summary>
        private readonly Dictionary<Type, MethodInfo> _handlers_HandleMethods;
        /// <summary>
        /// Thread safety object.
        /// </summary>
        private static SemaphoreSlim s_lock = new SemaphoreSlim(1);
        /// <summary>
        /// Current DI scope.
        /// </summary>
        private readonly IScope _scope;
        /// <summary>
        /// Collection of awaiters.
        /// </summary>
        private readonly ICollection<IEventAwaiter> _eventAwaiters;

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InMemoryStatelessEventBus()
        {
            _scope = DIManager.BeginScope();
            _events = new Dictionary<IDomainEvent, IEventContext>();
            _handlers_HandleMethods = new Dictionary<Type, MethodInfo>();
            _eventHandlers = ReflectionTools.GetAllTypes().Where(x => x.GetInterfaces()
                           .Any(y => y.GetTypeInfo().IsGenericType &&
                                     y.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
                           .ToList();
            _eventAwaiters = new List<IEventAwaiter>();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Treat all events according to rules.
        /// </summary>
        private async Task TreatEventsAsync()
        {
            if (_events.Any())
            {
                await s_lock.WaitAsync();
                try
                {
                    while (_events.Count > 0)
                    {
                        var evt = _events.ElementAt(0);
                        _events.Remove(evt.Key);
                        var evtType = evt.Key.GetType();
                        var handlers = GetHandlerForEventType(evtType).ToList();
                        if (handlers.Any())
                        {
                            foreach (var h in handlers)
                            {
                                var handlerInstance = GetOrCreateHandler(h, evt.Value);
                                if (handlerInstance != null)
                                {
                                    var handleMethod = h.GetTypeInfo().GetMethod("HandleAsync", new[] { evt.Key.GetType(), typeof(IEventContext) });
                                    await (Task)handleMethod.Invoke(handlerInstance, new object[] { evt.Key, evt.Value });
                                }
                                else
                                {
                                    Debug.WriteLine($"Cannot retrieve an instance of type {h.Name}. " +
                                        $"Event {evtType.Name} hasn't been sent.");
                                }
                            }
                        }
                    }
                }
                finally
                {
                    s_lock.Release();
                }
            }
        }

        /// <summary>
        /// Retrieve an handler for a particular event type.
        /// </summary>
        /// <param name="eventType">Type to retrieve handler types collection.</param>
        /// <returns>Collection of handler types.</returns>
        private IEnumerable<Type> GetHandlerForEventType(Type eventType)
        {
            var handlers = new List<Type>();

            handlers.AddRange(_eventHandlers.Where(e => e.GetInterfaces().Any(i =>
                i.GetTypeInfo().IsGenericType
             && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
             && i.GenericTypeArguments[0] == eventType)));

            return handlers;
        }

        /// <summary>
        /// Get or create and handler for a type.
        /// </summary>
        /// <param name="handlerType">Type of handler.</param>
        /// <param name="context">Context of event.</param>
        /// <returns>Instance of handler.</returns>
        private object GetOrCreateHandler(Type handlerType, IEventContext context)
        {
            object result = null;
            if (typeof(ISaga).IsAssignableFrom(handlerType))
            {
                if (context != null && context.GetType() == handlerType && !(context as ISaga).Completed)
                {
                    result = context;
                }
            }
            else if (typeof(IEventAwaiter).IsAssignableFrom(handlerType))
            {
                result = _eventAwaiters.FirstOrDefault(a => a.GetType() == handlerType);
            }
            else
            {
                if (context is IScope scope && !scope.IsDisposed)
                {
                    result = scope.Resolve(handlerType);

                }
                else if (context is IScopeHolder scopeHolder && !scopeHolder.Scope.IsDisposed)
                {
                    result = scopeHolder.Scope.Resolve(handlerType);
                }
                else
                {
                    result = _scope.Resolve(handlerType);
                }
                if (result == null)
                {
                    result = handlerType.CreateInstance();
                }

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
        public virtual Task RegisterAsync(IDomainEvent @event, IEventContext context = null)
        {
            _events.Add(@event, context);
            return TreatEventsAsync();
        }

        #endregion

    }
}
