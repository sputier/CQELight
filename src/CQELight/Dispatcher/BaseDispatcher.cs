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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.Dispatcher
{
    /// <summary>
    /// Dispatcher of events and commands.
    /// </summary>
    public class BaseDispatcher : IDispatcher
    {
        #region Static members

        private readonly IScope _scope;
        private readonly ILogger _logger;
        private readonly DispatcherConfiguration _config;

        #endregion

        #region ctor

        /// <summary>
        /// Create a new dispatcher instance.
        /// </summary>
        /// <param name="configuration">Dispatcher configuration.</param>
        /// <param name="scopeFactory">Factory of DI scope.</param>
        public BaseDispatcher(DispatcherConfiguration configuration, IScopeFactory scopeFactory = null)
        {
            _config = configuration ?? DispatcherConfiguration.Current;
            if (scopeFactory != null)
            {
                _scope = scopeFactory.CreateScope();
            }

            _logger =
                _scope?.Resolve<ILoggerFactory>()?.CreateLogger<BaseDispatcher>()
                ??
                new LoggerFactory().CreateLogger<BaseDispatcher>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Publish a range of events.
        /// </summary>
        /// <param name="data">Collection of events with their associated context.</param>
        /// <param name="callerMemberName">Caller name.</param>
        public async Task PublishEventsRangeAsync(IEnumerable<(IDomainEvent Event, IEventContext Context)> data,
            [CallerMemberName] string callerMemberName = "")
        {
            var tasks = new List<Task>();

            foreach (var element in data)
            {
                tasks.Add(PublishEventAsync(element.Event, element.Context, callerMemberName));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Publish a range of events.
        /// </summary>
        /// <param name="data">Collection of events.</param>
        /// <param name="callerMemberName">Caller name.</param>
        public Task PublishEventsRangeAsync(IEnumerable<IDomainEvent> events, [CallerMemberName] string callerMemberName = "")
            => PublishEventsRangeAsync(events.Select(e => (e, null as IEventContext)));

        /// <summary>
        /// Publish asynchronously an event and its context within every bus that it's configured for.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        /// <param name="context">Context to associate.</param>
        /// <param name="callerMemberName">Caller name.</param>
        public async Task PublishEventAsync(IDomainEvent @event, IEventContext context = null, [CallerMemberName] string callerMemberName = "")
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

            _logger.LogThreadInfos();

            var eventConfiguration = _config.EventDispatchersConfiguration.FirstOrDefault(e => new TypeEqualityComparer().Equals(e.EventType, @event.GetType()));
            await CoreDispatcher.PublishEventToSubscribers(@event, eventConfiguration.IsSecurityCritical).ConfigureAwait(false);

            foreach (var bus in eventConfiguration.BusesTypes)
            {
                try
                {
                    IDomainEventBus busInstance = null;
                    if (_scope != null)
                    {
                        busInstance = _scope.Resolve(bus) as IDomainEventBus;
                    }
                    else
                    {
                        busInstance = bus.CreateInstance() as IDomainEventBus;
                    }
                    if (busInstance != null)
                    {
                        _logger.LogInformation($"Dispatcher : Sending the event {eventType.FullName} on bus {bus.FullName}");
                        await busInstance.PublishEventAsync(@event, context).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.LogWarning($"Dispatcher : Instance of events bus {bus.FullName} cannot be retrieved from scope.");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogErrorMultilines($"Dispatcher.DispatchEventAsync() : Exception when sending event {eventType.FullName} on bus {bus.FullName}",
                        e.ToString());
                    eventConfiguration.ErrorHandler?.Invoke(e);
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
        public async Task DispatchCommandAsync(ICommand command, ICommandContext context = null, [CallerMemberName] string callerMemberName = "")
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

            _logger.LogThreadInfos();

            var commandConfiguration = _config.CommandDispatchersConfiguration.FirstOrDefault(e => e.CommandType == command.GetType());
            await CoreDispatcher.PublishCommandToSubscribers(command, commandConfiguration.IsSecurityCritical);

            var tasks = new List<Task>();

            foreach (var bus in commandConfiguration.BusesTypes)
            {
                try
                {
                    ICommandBus busInstance = null;
                    if (_scope != null)
                    {
                        busInstance = _scope.Resolve(bus) as ICommandBus;
                    }
                    else
                    {
                        busInstance = bus.CreateInstance() as ICommandBus;
                    }
                    if (busInstance != null)
                    {
                        _logger.LogInformation($"Dispatcher : Sending the command {commandType.FullName} on bus {bus.FullName}");
                        await busInstance.DispatchAsync(command, context).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.LogWarning($"Dispatcher : Instance of command bus {bus.FullName} cannot be retrieved from scope.");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogErrorMultilines($"Dispatcher.DispatchCommandAsync() : Exception when sending command {commandType.FullName} on bus {bus.FullName}",
                        e.ToString());
                    commandConfiguration.ErrorHandler?.Invoke(e);
                }
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        #endregion

        #region Private methods

        private bool EventTypeMatch(Type eventType, Type otherEventType)
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
