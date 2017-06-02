using CQELight.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Implementations.Events
{
    /// <summary>
    /// Implementation of bus in memory for domain events
    /// </summary>
    public class InMemoryDomainEventBus : IDomainEventBus
    {

        #region Members

        /// <summary>
        /// Instance of the bus.
        /// </summary>
        static readonly InMemoryDomainEventBus _instance;
        /// <summary>
        /// List of handlers for domain events.
        /// </summary>
        static readonly IEnumerable<Type> _handlers = new List<Type>();

        #endregion

        #region Static tor

        /// <summary>
        /// Static ctor.
        /// </summary>
        static InMemoryDomainEventBus()
        {
            _instance = new InMemoryDomainEventBus();
            _handlers = ReflectionExtension.GetAllTypes(typeof(InMemoryDomainEventBus).GetTypeInfo().Assembly.Location).Where(x => x.GetInterfaces()
                           .Any(y => y.GetTypeInfo().IsGenericType &&
                                     y.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)));
        }

        #endregion

        #region IDomainEventBus methods

        /// <summary>
        /// Dispatch a domain event into the system in memory.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        public void Dispatch(IDomainEvent @event)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Dispatch asynchronously a domain event into the system in memory.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        public Task DispatchAsync(IDomainEvent @event)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Dispatch a range of domain events into the system in memory.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        public void DispatchRange(IEnumerable<IDomainEvent> events)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Dispatch asynchronously a range of domain events into the system in memory.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        public Task DispatchRangeAsync(IEnumerable<IDomainEvent> events)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Static proxy method to dispatch event synchronously.
        /// </summary>
        /// <param name="event">Event to dispatch</param>
        public static void DispatchEvent(IDomainEvent @event)
            => _instance.Dispatch(@event);

        /// <summary>
        /// Dispatch asynchronously a domain event into the system in memory.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        public static Task DispatchEventAsync(IDomainEvent @event)
            => _instance.DispatchAsync(@event);

        /// <summary>
        /// Dispatch a range of domain events into the system in memory.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        public static void DispatchRangeEvents(IEnumerable<IDomainEvent> events)
            => _instance.DispatchRange(events);

        /// <summary>
        /// Dispatch asynchronously a range of domain events into the system in memory.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        public static Task DispatchRangeEventsAsync(IEnumerable<IDomainEvent> events)
            => _instance.DispatchRangeAsync(events);

        #endregion

    }
}
