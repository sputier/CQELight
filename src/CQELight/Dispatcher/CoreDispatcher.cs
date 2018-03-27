using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Dispatcher;
using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher.Configuration;
using CQELight.IoC;
using CQELight.Tools;
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

#pragma warning disable S3264 
        internal static event Func<IDomainEvent, Task> OnEventDispatched;
        internal static event Action<ICommand> OnCommandDispatched;
        internal static event Action<IMessage> OnMessageDispatched;
#pragma warning restore S3264 

        #endregion

        #region Static members

        private static IScope _dispatcherScope;
        internal readonly static IScope _scope;
        static readonly ILogger _logger;
        static CoreDispatcherConfiguration _config;
        static bool _isConfigured;
        static SemaphoreSlim s_HandlerManagementLock = new SemaphoreSlim(1);
        static ConcurrentBag<WeakReference<object>> s_EventHandlers = new ConcurrentBag<WeakReference<object>>();
        static ConcurrentBag<WeakReference<object>> s_CommandHandlers = new ConcurrentBag<WeakReference<object>>();
        static ConcurrentBag<WeakReference<object>> s_MessagesHandlers = new ConcurrentBag<WeakReference<object>>();
        static ConcurrentDictionary<Type, SemaphoreSlim> s_LockData = new ConcurrentDictionary<Type, SemaphoreSlim>();

        #endregion

        #region Static accessor

        static CoreDispatcher()
        {
            if (DIManager.IsInit)
            {
                _scope = DIManager.BeginScope();
                _logger = _scope.Resolve<ILoggerFactory>().CreateLogger(nameof(CoreDispatcher));
            }
            else
            {
                _logger = new LoggerFactory().CreateLogger(nameof(CoreDispatcher));
            }
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
            var (IsCommandHandler, IsEventHandler, IsMessageHandler) = GetHandlerTypeOf(handler);
            if (!IsEventHandler && !IsCommandHandler && !IsMessageHandler)
            {
                return;
            }
            s_HandlerManagementLock.Wait();
            try
            {
                LogThreadInfos();

                void RemoveHandlerFrom(ref ConcurrentBag<WeakReference<object>> collection)
                {
                    var handlerReference = collection.FirstOrDefault(c =>
                    {
                        if (c.TryGetTarget(out object h))
                            return h == handler;
                        return false;
                    });
                    if (handlerReference != null)
                    {
                        _logger.LogInformation($"Dispatcher : Remove an handler of type {handler.GetType()} from CoreDispatcher.");
                        collection = new ConcurrentBag<WeakReference<object>>(collection.Except(new[] { handlerReference }));
                    }
                };

                if (IsCommandHandler)
                {
                    RemoveHandlerFrom(ref s_CommandHandlers);
                }
                if (IsEventHandler)
                {
                    RemoveHandlerFrom(ref s_EventHandlers);
                }
                if (IsMessageHandler)
                {
                    RemoveHandlerFrom(ref s_MessagesHandlers);
                }
            }
            finally
            {
                s_HandlerManagementLock.Release();
            }
        }

        /// <summary>
        /// Add a staticly created instance of a specific handler directly in dispatcher. Will be used only in current process.
        /// </summary>
        /// <param name="handler">Handler instance.</param>
        public static void AddHandlerToDispatcher(object handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            var (IsCommandHandler, IsEventHandler, IsMessageHandler) = GetHandlerTypeOf(handler);
            if (!IsEventHandler && !IsCommandHandler && !IsMessageHandler)
            {
                return;
            }
            s_HandlerManagementLock.Wait();
            try
            {
                void AddHandlerIfNotExistsIn(ConcurrentBag<WeakReference<object>> collection)
                {
                    if (!collection.Any(c =>
                     {
                         c.TryGetTarget(out object h);
                         return h == handler;
                     }))
                    {
                        _logger.LogInformation($"Dispatcher : Adding an handler of type {handler.GetType()} in CoreDispatcher.");
                        collection.Add(new WeakReference<object>(handler));
                    }
                };

                if (IsCommandHandler)
                {
                    AddHandlerIfNotExistsIn(s_CommandHandlers);
                }
                if (IsEventHandler)
                {
                    AddHandlerIfNotExistsIn(s_EventHandlers);
                }
                if (IsMessageHandler)
                {
                    AddHandlerIfNotExistsIn(s_MessagesHandlers);
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
                    }
                    catch
                    {
                        // No need to stop if any error in logging
                    }
                    try
                    {
                        await act(@event).ConfigureAwait(false);
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
                    IDomainEventBus busInstance = null;
                    if (GetScope() != null)
                    {
                        busInstance = GetScope().Resolve(dispatcher.BusType) as IDomainEventBus;
                    }
                    else
                    {
                        busInstance = dispatcher.BusType.CreateInstance() as IDomainEventBus;
                    }
                    if (busInstance != null)
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
                    try
                    {
                        _logger.LogInformation($"Dispatcher : Invoke action {act.Method.Name} on {(act.Target != null ? act.Target.GetType().FullName : act.Method.DeclaringType.FullName)} " +
                            $"for command {commandType.FullName}");
                    }
                    catch
                    {
                        //No need to stop if any error in logging
                    }
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
                IEnumerable<ICommandBus> buses = null;
                if (GetScope() != null)
                {
                    buses = GetScope().ResolveAllInstancesOf<ICommandBus>();
                }
                else
                {
                    buses = ReflectionTools.GetAllTypes().Where(m => typeof(ICommandBus).IsAssignableFrom(m)).Select(b => b.CreateInstance()).WhereNotNull().Cast<ICommandBus>();
                }
                foreach (var bus in buses)
                {
                    _logger.LogInformation($"Dispatcher : Sending command {commandType.FullName} on bus {bus.GetType().FullName}");
                    tasks.AddRange(await bus.DispatchAsync(command, context));
                }
            }
            return awaiter;
        }

        /// <summary>
        /// Dispatch to all alive handlers a specific message.
        /// </summary>
        /// <param name="message">Instance of message to dispatch..</param>
        /// <param name="waitForCompletion">Flag that indicates if handlers can be run in parallel or if they should wait one after another for completion.</param>
        public static async Task DispatchMessageAsync<T>(T message, bool waitForCompletion = true)
            where T : IMessage
        {
            var sem = s_LockData.GetOrAdd(typeof(T), type => new SemaphoreSlim(1));
            await sem.WaitAsync(); // perform a lock per message type to allow parallel execution of different messages
            LogThreadInfos();
            _logger.LogInformation($"Dispatcher : Beginning of dispatch a message of type {typeof(T).FullName}");
            try
            {
#pragma warning disable CS4014
                Task.Run(() => _logger.LogDebug($"Dispatcher : Message's data = {Environment.NewLine}{message.ToJson()}"));
#pragma warning restore
            }
            catch
            {
                //No need to stop for logging
            }
            try
            {
                if (OnMessageDispatched != null)
                {
                    foreach (Action<IMessage> act in OnMessageDispatched.GetInvocationList().OfType<Action<IMessage>>())
                    {
                        try
                        {
                            _logger.LogInformation($"Dispatcher :Invoking action {act.Method.Name} on {act.Target.GetType().FullName} for " +
                                $"message of type {typeof(T).FullName}");
                        }
                        catch
                        {
                            //No stop if any error on log
                        }
                        try
                        {
                            act(message);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"CoreDispatcher.DispatchAppMessage() : Cannot invoke action {act.Method.Name} " +
                                $"on {act.Target.GetType().FullName} for message of type {typeof(T).FullName}. Exception data : {Environment.NewLine} {e}");
                        }
                    }
                }
                bool IsCompatible(Type vmType, Type eventType)
                {
                    return vmType.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>)
                           && i.GetGenericArguments().Contains(eventType));
                }

                var toDelete = new List<WeakReference<object>>();
                foreach (var vm in s_MessagesHandlers)
                {
                    if (vm.TryGetTarget(out object handler))
                    {
                        var handlerType = handler.GetType();
                        var messageType = message.GetType();
                        if (IsCompatible(handlerType, message.GetType()))
                        {
                            try
                            {
                                _logger.LogInformation($"Dispatcher : Invoking handle method on handler type {handlerType.FullName} " +
                                    $"for message of type {messageType.FullName}");
                                var methodInfo = handlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                                var method = Array.Find(methodInfo, m => m.Name == nameof(IMessageHandler<T>.HandleMessageAsync) && m.GetParameters()
                                        .Any(p => p.ParameterType == messageType));
                                if (method != null)
                                {
                                    var task = method.Invoke(handler, new object[] { message });
                                    if (waitForCompletion)
                                    {
                                        await (Task)(task);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogErrorMultilines("CoreDispatcher.DispatchAppMessage() : " +
                                    $"Cannot handle message on handler {handlerType.FullName} " +
                                    $"for message of type {messageType.FullName}", e.ToString());
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Dispatcher : Removing handler of type {vm.GetType().FullName} because instance has been garbage collected.");
                        toDelete.Add(vm);
                    }
                }
                await s_HandlerManagementLock.WaitAsync();
                try
                {
                    s_MessagesHandlers = new ConcurrentBag<WeakReference<object>>(s_MessagesHandlers.Except(toDelete));
                }
                finally
                {
                    s_HandlerManagementLock.Release();
                }

            }
            finally
            {
                sem.Release();
            }
        }

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

                if (toDelete.Count > 0)
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

                if (toDelete.Count > 0)
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

        internal static IScope GetScope()
        {
            if (_dispatcherScope == null || _dispatcherScope.IsDisposed)
            {
                _dispatcherScope = DIManager.BeginScope();
            }
            return _dispatcherScope;
        }

        internal static void UseConfiguration(CoreDispatcherConfiguration config)
        {
            _config = config;
            _isConfigured = true;
        }

        internal static void CleanRegistrations()
        {
            s_MessagesHandlers = new ConcurrentBag<WeakReference<object>>();
            s_CommandHandlers = new ConcurrentBag<WeakReference<object>>();
            s_EventHandlers = new ConcurrentBag<WeakReference<object>>();
        }


        #endregion

        #region Private methods

        private static (bool IsCommandHandler, bool IsEventHandler, bool IsMessageHandler) GetHandlerTypeOf(object handler)
        {
            Type handlerType = handler.GetType();
            bool isEventHandler = handlerType.ImplementsRawGenericInterface(typeof(IDomainEventHandler<>));
            bool isCommandHandler = handlerType.ImplementsRawGenericInterface(typeof(ICommandHandler<>));
            bool isMessageHandler = handlerType.ImplementsRawGenericInterface(typeof(IMessageHandler<>));

            return (isCommandHandler, isEventHandler, isMessageHandler);
        }

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
