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
using System.Threading.Tasks;

namespace CQELight.Implementations.Events.InMemory.Stateless
{
    /// <summary>
    /// InMemory stateless bus to dispatch events.
    /// If program shutdowns unexpectedly, it means all events stored in it are lost and cannot be retrieved. 
    /// However, this is a very fast bus for dispatch.
    /// </summary>
    public class InMemoryStatelessEventBus : IDomainEventBus, IConfigurableBus<InMemoryStatelessEventBusConfiguration>
    {

        #region Private members

        /// <summary>
        /// Collection of event handlers.
        /// </summary>
        private IEnumerable<Type> _eventHandlers;
        /// <summary>
        /// Current list of events to process.
        /// </summary>
        private Dictionary<IDomainEvent, IEventContext> _events;
        /// <summary>
        /// Thread safety object.
        /// </summary>
        private static object s_lock = new object();
        /// <summary>
        /// Cache for methodInfo.
        /// </summary>
        private Dictionary<Type, MethodInfo> _handlers_HandleMethods;
        /// <summary>
        /// Current DI scope.
        /// </summary>
        private IScope _scope;
        /// <summary>
        /// Collection of awaiters.
        /// </summary>
        private ICollection<IEventAwaiter> _eventAwaiters;
        /// <summary>
        /// Flag to indicates if bus is already initialized.
        /// </summary>
        private static bool s_alreadyInitialized;
        /// <summary>
        /// Current instance.
        /// </summary>
        private static InMemoryStatelessEventBus _instance;

        #endregion

        #region Properties

        /// <summary>
        /// Working flag.
        /// </summary>
        public bool Working
        {
            get;
            private set;
        }
        /// <summary>
        /// Current instance.
        /// </summary>
        internal static InMemoryStatelessEventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (s_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new InMemoryStatelessEventBus();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Waiting time between dispatchs.
        /// </summary>
        internal uint WaitTime { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        private InMemoryStatelessEventBus()
        {
            if (s_alreadyInitialized)
            {
                throw new InvalidOperationException("InMemoryStatefulEventBus.ctor() : InMemoryStatelessEventBus can be created only once.");
            }
            _scope = DIManager.BeginScope();
            _events = new Dictionary<IDomainEvent, IEventContext>();
            _handlers_HandleMethods = new Dictionary<Type, MethodInfo>();
            _eventHandlers = ReflectionTools.GetAllTypes().Where(x => x.GetInterfaces()
                           .Any(y => y.GetTypeInfo().IsGenericType &&
                                     y.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
                           .ToList();
            _eventAwaiters = new List<IEventAwaiter>();
            s_alreadyInitialized = true;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Stop the bus.
        /// </summary>
        public void Stop() => Working = false;

        /// <summary>
        /// Begin the bus.
        /// </summary>
        public void Start()
        {
            if (Working)
            {
                return;
            }
            if (WaitTime == default(ulong))
            {
                Configure(InMemoryStatelessEventBusConfiguration.Default);
            }
            Working = true;
            Task.Run(async () =>
            {
                while (Working)
                {
                    await Task.Delay((int)WaitTime).ConfigureAwait(false);
                    await TreatEventsAsync().ConfigureAwait(false);
                }
            });
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Treat all events according to rules.
        /// </summary>
        private Task TreatEventsAsync()
        {
            if (_events.Any())
            {
                lock (s_lock)
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
                                var name = h.Name;
                                var handlerInstance = GetOrCreateHandler(h, evt.Value);
                                if (handlerInstance != null)
                                {
                                    var handleMethod = h.GetTypeInfo().GetMethod("HandleAsync", new[] { evt.Key.GetType(), typeof(IEventContext) });
                                    //Impossible d'await dans un lock
                                    ((Task)handleMethod.Invoke(handlerInstance, new object[] { evt.Key, evt.Value })).GetAwaiter().GetResult();
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
            }
            return Task.FromResult(0);
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
            IEventAwaiter awaiter = _eventAwaiters.FirstOrDefault(e => e.GetType().GetTypeInfo().GenericTypeArguments[0] == eventType);
            if (awaiter == null)
            {
                awaiter = (IEventAwaiter)typeof(EventAwaiter<>).MakeGenericType(new[] { eventType }).CreateInstance();
                _eventAwaiters.Add(awaiter);
            }
            handlers.Add(awaiter.GetType());

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
                if (context != null && context.GetType() == handlerType)
                {
                    if (!(context as ISaga).Completed)
                    {
                        result = context;
                    }
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
        /// Register synchronously an event to be processed by the bus.
        /// </summary>
        /// <param name="event">Event to register.</param>
        /// <param name="context">Context associated to the event..</param>
        public virtual void Register(IDomainEvent @event, IEventContext context = null)
        {
            lock (s_lock)
            {
                _events.Add(@event, context);
            }
        }


        /// <summary>
        /// Register asynchronously an event to be processed by the bus.
        /// </summary>
        /// <param name="event">Event to register.</param>
        /// <param name="context">Context associated to the event..</param>
        public virtual Task RegisterAsync(IDomainEvent @event, IEventContext context = null)
        {
            Register(@event, context);
            return Task.FromResult(0);
        }

        #endregion

        #region IConfigurable methods

        /// <summary>
        /// Configure le bus avec la configuration donnée.
        /// </summary>
        /// <param name="config">Configuration de bus.</param>
        public void Configure(InMemoryStatelessEventBusConfiguration config)
        {
            if (config != null)
            {
                WaitTime = config.WaitTime;
            }
        }

        #endregion

    }
}
