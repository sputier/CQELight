using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.Attributes;
using CQELight.EventStore.CosmosDb.Common;
using CQELight.EventStore.CosmosDb.Models;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CQELight.EventStore.CosmosDb
{
    internal class CosmosDbEventStore : IEventStore, IAggregateEventStore
    {

        #region IEventStore methods

        /// <summary>
        /// Get a collection of events for a specific aggregate.
        /// </summary>
        /// <param name="aggregateUniqueId">Id of the aggregate which we want all the events.</param>
        /// <typeparam name="TAggregate">Aggregate type.</typeparam>
        /// <returns>Collection of all associated events.</returns>
        public Task<IEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TAggregate>(Guid aggregateUniqueId)
            where TAggregate : class
            => GetEventsFromAggregateIdAsync(aggregateUniqueId, typeof(TAggregate));

        /// <summary>
        /// Store a domain event in the event store
        /// </summary>
        /// <param name="event">Event instance to be persisted.</param>
        public Task StoreDomainEventAsync(IDomainEvent @event)
        {
            if (@event.GetType().IsDefined(typeof(EventNotPersistedAttribute)))
            {
                return Task.CompletedTask;
            }

            return SaveEvent(@event);
        }

        /// <summary>
        /// Get an event per its id.
        /// </summary>
        /// <param name="eventId">Id of the event.</param>
        /// <typeparam name="TEvent">Type of event to retrieve.</typeparam>
        /// <returns>Instance of the event.</returns>
        public Task<TEvent> GetEventByIdAsync<TEvent>(Guid eventId)
            where TEvent : class, IDomainEvent
            => Task.Run(()
                => EventStoreManager.GetRehydratedEventFromDbEvent(
                    EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.DatabaseLink)
                    .Where(@event => @event.Id == eventId).ToList().FirstOrDefault()) as TEvent);

        /// <summary>
        /// Get a collection of events for a specific aggregate.
        /// </summary>
        /// <param name="aggregateUniqueId">Id of the aggregate which we want all the events.</param>
        /// <param name="aggregateType">Type of the aggregate.</param>
        /// <returns>Collection of all associated events.</returns>
        public Task<IEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync(Guid aggregateUniqueId, Type aggregateType)
            => Task.Run(() => EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.DatabaseLink)
                  .Where(@event => @event.AggregateId == aggregateUniqueId && @event.AggregateType == aggregateType.AssemblyQualifiedName)
                  .Select(x => EventStoreManager.GetRehydratedEventFromDbEvent(x)).ToList().AsEnumerable());


        /// <summary>
        /// Retrieve a rehydrated aggregate from its unique Id and its type.
        /// </summary>
        /// <param name="aggregateUniqueId">Aggregate unique id.</param>
        /// <param name="aggregateType">Aggregate type.</param>
        /// <returns>Rehydrated event source aggregate.</returns>
        public Task<IEventSourcedAggregate> GetRehydratedAggregateAsync(Guid aggregateUniqueId, Type aggregateType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieve a rehydrated aggregate from its unique Id and its type.
        /// </summary>
        /// <param name="aggregateUniqueId">Aggregate unique id.</param>
        /// <returns>Rehydrated event source aggregate.</returns>
        /// <typeparam name="T">Type of aggregate to retrieve</typeparam>
        public Task<T> GetRehydratedAggregateAsync<T>(Guid aggregateUniqueId)
             where T : class, IEventSourcedAggregate, new()
        {
            throw new NotImplementedException();
        }

        #endregion        

        #region Private methods

        private Task SaveEvent(IDomainEvent @event)
        {
            var persistedEvent = new Event
            {
                AggregateId = @event.AggregateId,
                AggregateType = @event.AggregateType?.AssemblyQualifiedName,
                EventData = @event.ToJson(),
                EventTime = @event.EventTime,
                Id = @event.Id,
                Sequence = @event.Sequence,
                EventType = @event.GetType().AssemblyQualifiedName
            };
            return EventStoreAzureDbContext.Client.CreateDocumentAsync(EventStoreAzureDbContext.DatabaseLink, persistedEvent);
        }

        #endregion

    }
}
