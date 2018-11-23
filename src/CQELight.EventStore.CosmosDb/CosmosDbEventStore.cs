using CQELight.Abstractions.DDD;
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

        #region Private members

        private readonly ISnapshotBehaviorProvider _snapshotBehaviorProvider;

        #endregion

        #region Ctor

        public CosmosDbEventStore(ISnapshotBehaviorProvider snapshotBehaviorProvider = null)
        {
            _snapshotBehaviorProvider = snapshotBehaviorProvider;
        }

        #endregion

        #region IEventStore methods

        /// <summary>
        /// Get a collection of events for a specific aggregate.
        /// </summary>
        /// <param name="aggregateUniqueId">Id of the aggregate which we want all the events.</param>
        /// <typeparam name="TAggregate">Aggregate type.</typeparam>
        /// <returns>Collection of all associated events.</returns>
        public Task<IAsyncEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TAggregate>(Guid aggregateUniqueId)
            where TAggregate : class
            => GetEventsFromAggregateIdAsync(aggregateUniqueId, typeof(TAggregate));

        /// <summary>
        /// Store a domain event in the event store
        /// </summary>
        /// <param name="event">Event instance to be persisted.</param>
        public async Task StoreDomainEventAsync(IDomainEvent @event)
        {
            var eventType = @event.GetType();
            if (eventType.IsDefined(typeof(EventNotPersistedAttribute)))
            {
                return;
            }

            int currentSeq = -1;
            if (@event.AggregateId.HasValue)
            {
                ISnapshotBehavior behavior = _snapshotBehaviorProvider?.GetBehaviorForEventType(eventType);
                if (behavior != null && await behavior.IsSnapshotNeededAsync(@event.AggregateId.Value, @event.AggregateType).ConfigureAwait(false))
                {
                    var result = await behavior.GenerateSnapshotAsync(@event.AggregateId.Value, @event.AggregateType).ConfigureAwait(false);
                    if (result.Snapshot != null)
                    {
                        await EventStoreAzureDbContext.Client.CreateDocumentAsync(EventStoreAzureDbContext.SnapshotDatabaseLink, result.Snapshot).ConfigureAwait(false);
                    }
                }
                currentSeq = EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.EventsDatabaseLink)
                    .Where(e => e.AggregateId == @event.AggregateId.Value && e.AggregateType == @event.AggregateType.AssemblyQualifiedName).Count();
            }

            await SaveEvent(@event, ++currentSeq).ConfigureAwait(false);
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
                    EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.EventsDatabaseLink)
                    .Where(@event => @event.Id == eventId).ToList().FirstOrDefault()) as TEvent);

        /// <summary>
        /// Get a collection of events for a specific aggregate.
        /// </summary>
        /// <param name="aggregateUniqueId">Id of the aggregate which we want all the events.</param>
        /// <param name="aggregateType">Type of the aggregate.</param>
        /// <returns>Collection of all associated events.</returns>
        public Task<IAsyncEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync(Guid aggregateUniqueId, Type aggregateType)
            => Task.FromResult(EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.EventsDatabaseLink)
                  .Where(@event => @event.AggregateId == aggregateUniqueId && @event.AggregateType == aggregateType.AssemblyQualifiedName)
                  .ToList().Select(EventStoreManager.GetRehydratedEventFromDbEvent).ToAsyncEnumerable());


        /// <summary>
        /// Retrieve a rehydrated aggregate from its unique Id and its type.
        /// </summary>
        /// <param name="aggregateUniqueId">Aggregate unique id.</param>
        /// <param name="aggregateType">Aggregate type.</param>
        /// <returns>Rehydrated event source aggregate.</returns>
        public async Task<IEventSourcedAggregate> GetRehydratedAggregateAsync(Guid aggregateUniqueId, Type aggregateType)
        {
            if (aggregateType == null)
            {
                throw new ArgumentNullException(nameof(aggregateType));
            }
            if (aggregateUniqueId == Guid.Empty)
            {
                throw new ArgumentException("CosmosDbEventStore.GetRehydratedAggregate() : Id cannot be empty.");
            }
            if (!(aggregateType.CreateInstance() is IEventSourcedAggregate aggInstance))
            {
                throw new InvalidOperationException("CosmosDbEventStore.GetRehydratedAggregateAsync() : Cannot create a new instance of" +
                    $" {aggregateType.FullName} aggregate. It should have one parameterless constructor (can be private).");
            }

            var events = await(await GetEventsFromAggregateIdAsync(aggregateUniqueId, aggregateType).ConfigureAwait(false)).ToList().ConfigureAwait(false);
            var snapshot = await EventStoreAzureDbContext.Client.CreateDocumentQuery<Snapshot>(EventStoreAzureDbContext.SnapshotDatabaseLink)
                .Where(t => t.AggregateType == aggregateType.AssemblyQualifiedName && t.AggregateId == aggregateUniqueId)
                .ToAsyncEnumerable().FirstOrDefault().ConfigureAwait(false);

            PropertyInfo stateProp = aggregateType.GetAllProperties().FirstOrDefault(p => p.PropertyType.IsSubclassOf(typeof(AggregateState)));
            FieldInfo stateField = aggregateType.GetAllFields().FirstOrDefault(f => f.FieldType.IsSubclassOf(typeof(AggregateState)));
            Type stateType = stateProp?.PropertyType ?? stateField?.FieldType;
            if (stateType != null)
            {
                object state = null;
                if (snapshot != null)
                {
                    state = snapshot.SnapshotData.FromJson(stateType);
                }
                else
                {
                    state = stateType.CreateInstance();
                }

                if (stateProp != null)
                {
                    stateProp.SetValue(aggInstance, state);
                }
                else
                {
                    stateField.SetValue(aggInstance, state);
                }
            }
            else
            {
                throw new InvalidOperationException("CosmosDbEventStore.GetRehydratedAggregateAsync() : Cannot find property/field that manage state for aggregate" +
                    $" type {aggregateType.FullName}. State should be a property or a field of the aggregate");
            }
            aggInstance.RehydrateState(events);

            return aggInstance;
        }

        /// <summary>
        /// Retrieve a rehydrated aggregate from its unique Id and its type.
        /// </summary>
        /// <param name="aggregateUniqueId">Aggregate unique id.</param>
        /// <returns>Rehydrated event source aggregate.</returns>
        /// <typeparam name="T">Type of aggregate to retrieve</typeparam>
        public async Task<T> GetRehydratedAggregateAsync<T>(Guid aggregateUniqueId)
             where T : class, IEventSourcedAggregate, new()
            => (await GetRehydratedAggregateAsync(aggregateUniqueId, typeof(T)).ConfigureAwait(false)) as T;

        #endregion        

        #region Private methods

        private async Task SaveEvent(IDomainEvent @event, long currentSeq)
        {
            var persistedEvent = new Event
            {
                AggregateId = @event.AggregateId,
                AggregateType = @event.AggregateType?.AssemblyQualifiedName,
                EventData = @event.ToJson(),
                EventTime = @event.EventTime,
                Id = @event.Id,
                Sequence = (ulong)currentSeq,
                EventType = @event.GetType().AssemblyQualifiedName
            };
            var response = await EventStoreAzureDbContext.Client.CreateDocumentAsync(EventStoreAzureDbContext.EventsDatabaseLink, persistedEvent)
                .ConfigureAwait(false);
        }

        #endregion

    }
}
