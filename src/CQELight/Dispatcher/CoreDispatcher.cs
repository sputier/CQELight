using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Dispatcher;
using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher.Configuration;
using CQELight.IoC;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using Force.DeepCloner;
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
    /// The core dispatcher is the main dispatcher of the app. It is use for ease to work and manage all static references.
    /// </summary>
    public static class CoreDispatcher
    {

        #region Events

#pragma warning disable S3264 
        /// <summary>
        /// Custom callback when an event is published.
        /// </summary>
        public static event Func<IDomainEvent, Task> OnEventDispatched;
        /// <summary>
        /// Custom callback when a command is dispatched.
        /// </summary>
        public static event Action<ICommand> OnCommandDispatched;
        /// <summary>
        /// Custom callback when a message is dispatched.
        /// </summary>
        public static event Action<IMessage> OnMessageDispatched;
#pragma warning restore S3264 

        #endregion

        #region Static members

        static readonly ILogger _logger;
        static IDispatcher _instance;
        static SemaphoreSlim s_HandlerManagementLock = new SemaphoreSlim(1);
        static ConcurrentDictionary<Type, SemaphoreSlim> s_LockData = new ConcurrentDictionary<Type, SemaphoreSlim>();

        internal readonly static IScope _scope;
        internal static ConcurrentBag<WeakReference<object>> s_EventHandlers = new ConcurrentBag<WeakReference<object>>();
        internal static ConcurrentBag<WeakReference<object>> s_CommandHandlers = new ConcurrentBag<WeakReference<object>>();
        internal static ConcurrentBag<WeakReference<object>> s_MessagesHandlers = new ConcurrentBag<WeakReference<object>>();
        internal static ConcurrentBag<WeakReference<object>> s_TransactionnalHandlers = new ConcurrentBag<WeakReference<object>>();

        #endregion

        #region Static accessor

        static CoreDispatcher()
        {
            _instance = new Dispatcher(DispatcherConfiguration.Default);
            if (DIManager.IsInit)
            {
                _logger = DIManager.BeginScope().Resolve<ILoggerFactory>()?.CreateLogger("CoreDispatcher");
            }
            if (_logger == null)
            {
                _logger = new LoggerFactory().AddDebug().CreateLogger("CoreDispatcher");
            }
        }

        #endregion

        #region Public static mehtods

        /// <summary>
        /// Remove a handler instance from dispatcher.
        /// </summary>
        /// <param name="handler">Instance to delete.</param>
        public static void RemoveHandlerFromDispatcher(object handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            var (IsCommandHandler, IsEventHandler, IsMessageHandler, IsTransactionnalEventHandler) = GetHandlerTypeOf(handler);
            if (!IsEventHandler && !IsCommandHandler && !IsMessageHandler && !IsTransactionnalEventHandler)
            {
                return;
            }
            s_HandlerManagementLock.Wait();
            try
            {
                _logger.LogThreadInfos();

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
                        _logger.LogInformation($"Dispatcher : Remove an handler of type {handler.GetType()} from Dispatcher.");
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
                if (IsTransactionnalEventHandler)
                {
                    RemoveHandlerFrom(ref s_TransactionnalHandlers);
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
            var (IsCommandHandler, IsEventHandler, IsMessageHandler, IsTransactionnalEventHandler) = GetHandlerTypeOf(handler);
            if (!IsEventHandler && !IsCommandHandler && !IsMessageHandler && !IsTransactionnalEventHandler)
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
                        _logger.LogInformation($"Dispatcher : Adding an handler of type {handler.GetType()} in Dispatcher.");
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
                if (IsTransactionnalEventHandler)
                {
                    AddHandlerIfNotExistsIn(s_TransactionnalHandlers);
                }
                _logger.LogThreadInfos();
            }
            finally
            {
                s_HandlerManagementLock.Release();
            }
        }
        /// <summary>
        /// Publish a range of events.
        /// </summary>
        /// <param name="data">Collection of events with their associated context.</param>
        /// <param name="callerMemberName">Caller name.</param>
        public static Task PublishEventRangeAsync(IEnumerable<(IDomainEvent Event, IEventContext Context)> data, [CallerMemberName] string callerMemberName = "")
            => _instance.PublishEventRangeAsync(data, callerMemberName);

        /// <summary>
        /// Publish asynchronously an event and its context within every bus that it's configured for.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        /// <param name="context">Context to associate.</param>
        /// <param name="callerMemberName">Caller name.</param>
        public static Task PublishEventAsync(IDomainEvent @event, IEventContext context = null, [CallerMemberName] string callerMemberName = "")
            => _instance.PublishEventAsync(@event, context, callerMemberName);

        /// <summary>
        /// Dispatch asynchronously a command and its context within every bus that it's configured for.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="context">Context to associate.</param>
        /// <param name="callerMemberName">Calling method.</param>
        /// <returns>Awaiter of events.</returns>
        public static Task<DispatcherAwaiter> DispatchCommandAsync(ICommand command, ICommandContext context = null, [CallerMemberName] string callerMemberName = "")
            => _instance.DispatchCommandAsync(command, context, callerMemberName);

        /// <summary>
        /// Dispatch to all in-memory alive handlers a specific message.
        /// </summary>
        /// <param name="message">Instance of message to dispatch..</param>
        /// <param name="waitForCompletion">Flag that indicates if handlers can be run in parallel or if they should wait one after another for completion.</param>
        public static async Task DispatchMessageAsync<T>(T message, bool waitForCompletion = true)
            where T : IMessage
        {
            var sem = s_LockData.GetOrAdd(typeof(T), type => new SemaphoreSlim(1));
            await sem.WaitAsync().ConfigureAwait(false); // perform a lock per message type to allow parallel execution of different messages
            _logger.LogThreadInfos();
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
                            _logger.LogError($"Dispatcher.DispatchAppMessage() : Cannot invoke action {act.Method.Name} " +
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
                foreach (var vm in CoreDispatcher.s_MessagesHandlers)
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
                                _logger.LogErrorMultilines("Dispatcher.DispatchAppMessage() : " +
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
                await s_HandlerManagementLock.WaitAsync().ConfigureAwait(false);
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
                foreach (var handler in s_EventHandlers.Concat(s_TransactionnalHandlers))
                {
                    if (handler.TryGetTarget(out object instance))
                    {
                        if (instance.GetType().GetInterfaces()
                            .Any(i => i.IsGenericType
                                   && (i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>) || i.GetGenericTypeDefinition() == typeof(ITransactionnalEventHandler<>))
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
        public static IEnumerable<object> TryGetHandlersForCommandType(Type type)
        {
            s_HandlerManagementLock.Wait();
            try
            {
                List<object> result = new List<object>();
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
                            result.Add(instance);
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
                return result.AsEnumerable();
            }
            finally
            {
                s_HandlerManagementLock.Release();
            }
        }

        #endregion

        #region Internal static methods

        internal static async Task PublishEventToSubscribers(IDomainEvent @event, bool isEventSecurityCritical = false)
        {
            if (OnEventDispatched != null)
            {
                var eventType = @event.GetType();
                IDomainEvent eventInstance = @event;
                if (isEventSecurityCritical)
                {
                    eventInstance = @event.DeepClone();
                }
                foreach (Func<IDomainEvent, Task> act in OnEventDispatched.GetInvocationList().OfType<Func<IDomainEvent, Task>>())
                {
                    try
                    {
                        _logger.LogInformation($"CoreDispatcher : Invoke of action {act.Method.Name} on" +
                            $" {(act.Target != null ? act.Target.GetType().FullName : act.Method.DeclaringType.FullName)} for event {eventType.FullName}");
                    }
                    catch
                    {
                        // No need to stop if any error in logging
                    }
                    try
                    {
                        await act(eventInstance).ConfigureAwait(false);
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
        }

        internal static async Task PublishCommandToSubscribers(ICommand command, bool isCommandSecurityCritical = false)
        {
            if (OnCommandDispatched != null)
            {
                var commandType = command.GetType();
                ICommand commandInstance = command;
                if (isCommandSecurityCritical)
                {
                    commandInstance = command.DeepClone();
                }
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
                        await act(commandInstance).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _logger.LogErrorMultilines($"Dispatcher.DispatchCommandAsync() : " +
                            $"Cannot call action {act.Method.Name} on {(act.Target != null ? act.Target.GetType().FullName : act.Method.DeclaringType.FullName)} for command {commandType.FullName}",
                            e.ToString());
                    }
                }
            }
        }

        internal static void UseConfiguration(DispatcherConfiguration config) => _instance = new Dispatcher(config);

        internal static void CleanRegistrations()
        {
            s_MessagesHandlers = new ConcurrentBag<WeakReference<object>>();
            s_CommandHandlers = new ConcurrentBag<WeakReference<object>>();
            s_EventHandlers = new ConcurrentBag<WeakReference<object>>();
        }


        #endregion

        #region Private static methods

        private static (bool IsCommandHandler, bool IsEventHandler, bool IsMessageHandler, bool IsTransactionnalEventHandler) GetHandlerTypeOf(object handler)
        {
            Type handlerType = handler.GetType();
            bool isEventHandler = handlerType.ImplementsRawGenericInterface(typeof(IDomainEventHandler<>));
            bool isCommandHandler = handlerType.ImplementsRawGenericInterface(typeof(ICommandHandler<>));
            bool isMessageHandler = handlerType.ImplementsRawGenericInterface(typeof(IMessageHandler<>));
            bool isTransactionnalEventHandler = handlerType.ImplementsRawGenericInterface(typeof(ITransactionnalEventHandler<>));

            return (isCommandHandler, isEventHandler, isMessageHandler, isTransactionnalEventHandler);
        }
        #endregion

    }
}
