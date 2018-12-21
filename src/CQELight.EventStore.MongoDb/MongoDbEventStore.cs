using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.Attributes;
using CQELight.EventStore.MongoDb.Models;
using CQELight.Tools.Extensions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.EventStore.MongoDb
{
    /// <summary>
    /// MongoDb Event store implementation.
    /// </summary>
    public class MongoDbEventStore : IEventStore, IAggregateEventStore
    {
        #region Private members

        private readonly ISnapshotBehaviorProvider _snapshotBehaviorProvider;
        private readonly SnapshotEventsArchiveBehavior _archiveBehavior;

        #endregion

        #region Private methods

        private async Task<ISnapshot> GetSnapshotFromAggregateId(Guid aggregateId, Type aggregateType)
        {
            var filterBuilder = Builders<ISnapshot>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(nameof(ISnapshot.AggregateType), aggregateType.AssemblyQualifiedName),
                filterBuilder.Eq(nameof(ISnapshot.AggregateId), aggregateId));

            var snapshotCollection = await GetSnapshotCollectionAsync().ConfigureAwait(false); ;
            return await snapshotCollection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        private async Task<IMongoCollection<T>> GetEventCollectionAsync<T>()
            where T : IDomainEvent
        {
            var collection = EventStoreManager.Client
                      .GetDatabase(Consts.CONST_DB_NAME)
                      .GetCollection<T>(Consts.CONST_EVENTS_COLLECTION_NAME);

            await CreateEventCollectionDefaultIndexes(collection).ConfigureAwait(false);

            return collection;
        }

        private async Task<IMongoCollection<T>> GetArchiveDatabaseEventCollectionAsync<T>()
            where T : IDomainEvent

        {
            var collection = EventStoreManager.Client
                .GetDatabase(Consts.CONST_ARCHIVE_DB_NAME)
                .GetCollection<T>(Consts.CONST_ARCHIVE_EVENTS_COLLECTION_NAME);

            await CreateEventCollectionDefaultIndexes(collection).ConfigureAwait(false);

            return collection;
        }
        private async Task<IMongoCollection<T>> GetArchiveEventCollectionAsync<T>()
            where T : IDomainEvent
        {
            var collection = EventStoreManager.Client
                      .GetDatabase(Consts.CONST_DB_NAME)
                      .GetCollection<T>(Consts.CONST_ARCHIVE_EVENTS_COLLECTION_NAME);

            await CreateEventCollectionDefaultIndexes(collection).ConfigureAwait(false);

            return collection;
        }

        private async Task CreateEventCollectionDefaultIndexes<T>(IMongoCollection<T> collection)
            where T : IDomainEvent
        {

            await collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<T>(Builders<T>.IndexKeys
                    .Ascending(nameof(IDomainEvent.AggregateId))
                    .Ascending(nameof(IDomainEvent.AggregateType))
                    .Ascending(nameof(IDomainEvent.Sequence)))
                ).ConfigureAwait(false);
        }

        private async Task<IMongoCollection<ISnapshot>> GetSnapshotCollectionAsync()
        {
            var collection = EventStoreManager.Client
                .GetDatabase(Consts.CONST_DB_NAME)
                .GetCollection<ISnapshot>(Consts.CONST_SNAPSHOT_COLLECTION_NAME);

            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ISnapshot>(Builders<ISnapshot>.IndexKeys
                                                                    .Ascending(nameof(ISnapshot.AggregateId))
                                                                    .Ascending(nameof(ISnapshot.AggregateType))
                                                                    .Ascending(nameof(ISnapshot.SnapshotTime)))
                ).ConfigureAwait(false);

            return collection;
        }

        private async Task SetSequenceAsync(IDomainEvent @event)
        {
            long currentSeq = 0;
            var sequenceProp = @event.GetType().GetAllProperties().FirstOrDefault(m => m.Name == nameof(IDomainEvent.Sequence));
            if (@event.AggregateId.HasValue && sequenceProp?.SetMethod != null)
            {
                var filter = Builders<IDomainEvent>.Filter.Eq(nameof(IDomainEvent.AggregateId), @event.AggregateId.Value);
                var collection = await GetEventCollectionAsync<IDomainEvent>().ConfigureAwait(false);
                currentSeq = await collection.CountDocumentsAsync(filter).ConfigureAwait(false);
                sequenceProp.SetValue(@event, Convert.ToUInt64(++currentSeq));
            }
        }

        private void CheckIdAndSetNewIfNeeded(IDomainEvent @event)
        {
            if (@event.Id == Guid.Empty)
            {
                var idProp = @event.GetType().GetAllProperties().FirstOrDefault(p => p.Name == nameof(IDomainEvent.Id));
                if (idProp?.SetMethod != null)
                {
                    idProp.SetValue(@event, Guid.NewGuid());
                }
            }
        }

        #endregion

        #region Ctor

        public MongoDbEventStore(ISnapshotBehaviorProvider snapshotBehaviorProvider = null,
            SnapshotEventsArchiveBehavior archiveBehavior = SnapshotEventsArchiveBehavior.StoreToNewTable)
        {
            _snapshotBehaviorProvider = snapshotBehaviorProvider;
            _archiveBehavior = archiveBehavior;
        }

        #endregion

        #region IEventStore

        /// <summary>
        /// Get a collection of events for a specific aggregate.
        /// </summary>
        /// <param name="aggregateUniqueId">Id of the aggregate which we want all the events.</param>
        /// <param name="aggregateType">Type of the aggregate.</param>
        /// <returns>Collection of all associated events.</returns>
        public async Task<IAsyncEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync(Guid aggregateUniqueId, Type aggregateType)
        {
            var filterBuilder = Builders<IDomainEvent>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(nameof(IDomainEvent.AggregateId), aggregateUniqueId),
                filterBuilder.Eq(nameof(IDomainEvent.AggregateType), aggregateType));

            var collection = await GetEventCollectionAsync<IDomainEvent>().ConfigureAwait(false);

            var result = await collection.Find(filter).Sort(Builders<IDomainEvent>.Sort.Ascending(nameof(IDomainEvent.Sequence))).ToListAsync().ConfigureAwait(false);

            return result.ToAsyncEnumerable();
        }

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
            var evtType = @event.GetType();
            if (evtType.IsDefined(typeof(EventNotPersistedAttribute)))
            {
                return;
            }
            ISnapshotBehavior behavior = _snapshotBehaviorProvider?.GetBehaviorForEventType(evtType);
            if (behavior != null && await behavior.IsSnapshotNeededAsync(@event.AggregateId.Value, @event.AggregateType)
                .ConfigureAwait(false))
            {
                var snapshotCollection = await GetSnapshotCollectionAsync().ConfigureAwait(false);
                var filterBuilder = Builders<ISnapshot>.Filter;
                var filter = filterBuilder.And(
                    filterBuilder.Eq(nameof(ISnapshot.AggregateId), @event.AggregateId),
                    filterBuilder.Eq(nameof(ISnapshot.AggregateType), @event.AggregateType.AssemblyQualifiedName));
                var existingSnapshot = await (await snapshotCollection.FindAsync(filter).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);

                var result = await behavior.GenerateSnapshotAsync(@event.AggregateId.Value, @event.AggregateType, existingSnapshot).ConfigureAwait(false);
                if (result.Snapshot != null)
                {
                    await InsertSnapshotAsync(result.Snapshot).ConfigureAwait(false);
                }
                if (result.ArchiveEvents?.Any() == true)
                {
                    await StoreArchiveEventsAsync(result.ArchiveEvents).ConfigureAwait(false);
                }
            }

            CheckIdAndSetNewIfNeeded(@event);
            await SetSequenceAsync(@event).ConfigureAwait(false);

            var collection = await GetEventCollectionAsync<IDomainEvent>().ConfigureAwait(false);
            await collection.InsertOneAsync(@event).ConfigureAwait(false);
        }

        /// <summary>
        /// Get an event per its id.
        /// </summary>
        /// <param name="eventId">Id of the event.</param
        /// <typeparam name="TEvent">Type of event to retrieve.</typeparam>
        /// <returns>Instance of the event.</returns>
        public async Task<TEvent> GetEventByIdAsync<TEvent>(Guid eventId)
            where TEvent : class, IDomainEvent
        {
            var filter = Builders<TEvent>.Filter.Eq(e => e.Id, eventId);
            var collection = await GetEventCollectionAsync<TEvent>().ConfigureAwait(false);
            return await collection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        #endregion

        #region IAggregateEventStore

        public async Task<IEventSourcedAggregate> GetRehydratedAggregateAsync(Guid aggregateUniqueId, Type aggregateType)
        {
            if (aggregateType == null)
            {
                throw new ArgumentNullException(nameof(aggregateType));
            }
            if (aggregateUniqueId == Guid.Empty)
            {
                throw new ArgumentException("MongoDbEventStore.GetRehydratedAggregate() : Id cannot be empty.");
            }

            var events = await GetEventsFromAggregateIdAsync(aggregateUniqueId, aggregateType).ConfigureAwait(false);

            var snapshot = await GetSnapshotFromAggregateId(aggregateUniqueId, aggregateType).ConfigureAwait(false);

            if (!(aggregateType.CreateInstance() is IEventSourcedAggregate aggInstance))
            {
                throw new InvalidOperationException("MongoDbEventStore.GetRehydratedAggregateAsync() : Cannot create a new instance of" +
                    $" {aggregateType.FullName} aggregate. It should have one parameterless constructor (can be private).");
            }

            PropertyInfo stateProp = aggregateType.GetAllProperties().FirstOrDefault(p => p.PropertyType.IsSubclassOf(typeof(AggregateState)));
            FieldInfo stateField = aggregateType.GetAllFields().FirstOrDefault(f => f.FieldType.IsSubclassOf(typeof(AggregateState)));
            Type stateType = stateProp?.PropertyType ?? stateField?.FieldType;
            if (stateType != null)
            {
                object state = null;
                if (snapshot != null)
                {
                    state = snapshot.AggregateState;
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
                throw new InvalidOperationException("MongoDbEventStore.GetRehydratedAggregateAsync() : Cannot find property/field that manage state for aggregate" +
                    $" type {aggregateType.FullName}. State should be a property or a field of the aggregate");
            }
            aggInstance.RehydrateState(await events.ToList().ConfigureAwait(false));

            return aggInstance;
        }

        public async Task<T> GetRehydratedAggregateAsync<T>(Guid aggregateUniqueId) where T : class, IEventSourcedAggregate, new()
            => (await GetRehydratedAggregateAsync(aggregateUniqueId, typeof(T)).ConfigureAwait(false)) as T;


        #endregion

        #region Private methods

        private async Task StoreArchiveEventsAsync(IEnumerable<IDomainEvent> archiveEvents)
        {
            var filterBuilder = Builders<IDomainEvent>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(nameof(IDomainEvent.AggregateId), archiveEvents.First().AggregateId),
                filterBuilder.Eq(nameof(IDomainEvent.AggregateType), archiveEvents.First().AggregateType.AssemblyQualifiedName));

            switch (_archiveBehavior)
            {
                case SnapshotEventsArchiveBehavior.StoreToNewDatabase:
                    var otherDbArchiveCollection = await GetArchiveDatabaseEventCollectionAsync<IDomainEvent>().ConfigureAwait(false);
                    await otherDbArchiveCollection.InsertManyAsync(archiveEvents).ConfigureAwait(false);
                    break;
                case SnapshotEventsArchiveBehavior.StoreToNewTable:
                    var archiveCollection = await GetArchiveEventCollectionAsync<IDomainEvent>().ConfigureAwait(false);
                    await archiveCollection.InsertManyAsync(archiveEvents).ConfigureAwait(false);
                    break;
            }

            var deleteFilterBuilder = new FilterDefinitionBuilder<IDomainEvent>();
            var deleteFilter = deleteFilterBuilder.And(filter, deleteFilterBuilder.Lte(nameof(IDomainEvent.Sequence), archiveEvents.Max(e => e.Sequence)));
            await (await GetEventCollectionAsync<IDomainEvent>().ConfigureAwait(false)).DeleteManyAsync(deleteFilter).ConfigureAwait(false);
        }

        private async Task InsertSnapshotAsync(ISnapshot snapshot)
        {
            var snapshotCollection = await GetSnapshotCollectionAsync().ConfigureAwait(false);

            var filterBuilder = Builders<ISnapshot>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(nameof(ISnapshot.AggregateId), snapshot.AggregateId),
                filterBuilder.Eq(nameof(ISnapshot.AggregateType), snapshot.AggregateType));

            var existingSnapshot = await snapshotCollection.FindOneAndDeleteAsync(filter).ConfigureAwait(false);

            await snapshotCollection.InsertOneAsync(snapshot).ConfigureAwait(false);
        }

        #endregion

    }
}
