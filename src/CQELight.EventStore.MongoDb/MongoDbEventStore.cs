using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.Attributes;
using CQELight.EventStore.MongoDb.Models;
using CQELight.Tools.Extensions;
using MongoDB.Bson;
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

        private async Task<ISnapshot> GetSnapshotFromAggregateId<TId>(TId aggregateId, Type aggregateType)
        {
            var filterBuilder = Builders<ISnapshot>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(nameof(ISnapshot.AggregateType), aggregateType.AssemblyQualifiedName),
                filterBuilder.Eq(nameof(ISnapshot.AggregateId), (object)aggregateId));

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

        private async Task SetSequenceAsync(IDomainEvent @event, ulong? sequence = null)
        {
            if (@event.Sequence == 0 && @event.AggregateId != null)
            {
                ulong currentSeq = 0;
                if (sequence.HasValue)
                {
                    currentSeq = sequence.Value;
                }
                else
                {
                    async Task<long> RetrieveCurrentSequenceFromDbAsync()
                    {
                        var filter = Builders<IDomainEvent>.Filter.Eq(nameof(IDomainEvent.AggregateId), @event.AggregateId);
                        var collection = await GetEventCollectionAsync<IDomainEvent>().ConfigureAwait(false);
                        var filteredCollection = await collection.FindAsync(filter).ConfigureAwait(false);
                        var maxSequence = filteredCollection.ToList().Max(e => (ulong?)e.Sequence) ?? 0;
                        return Convert.ToInt64(maxSequence);
                    }
                    currentSeq = (ulong)(await RetrieveCurrentSequenceFromDbAsync().ConfigureAwait(false));
                    currentSeq++;
                }
                if (@event is BaseDomainEvent bde)
                {
                    bde.Sequence = currentSeq;
                }
                else
                {
                    var sequenceProp = @event.GetType().GetAllProperties().FirstOrDefault(m => m.Name == nameof(IDomainEvent.Sequence));
                    if (sequenceProp?.SetMethod != null)
                    {
                        sequenceProp.SetValue(@event, Convert.ToUInt64(currentSeq));
                    }
                    //TODO we preferably must log warning here
                }
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

        public IAsyncEnumerable<IDomainEvent> GetAllEventsByAggregateType(Type aggregateType)
        {
            var filterBuilder = Builders<IDomainEvent>.Filter;
            var filter = filterBuilder.Eq(nameof(IDomainEvent.AggregateType), aggregateType);

            var collection = EventStoreManager.Client
                            .GetDatabase(Consts.CONST_DB_NAME)
                            .GetCollection<IDomainEvent>(Consts.CONST_EVENTS_COLLECTION_NAME);

            return collection.Find(filter).ToEnumerable().ToAsyncEnumerable();
        }

        public IAsyncEnumerable<T> GetAllEventsByEventType<T>()
            where T : class, IDomainEvent
            => EventStoreManager.Client
                            .GetDatabase(Consts.CONST_DB_NAME)
                            .GetCollection<T>(Consts.CONST_EVENTS_COLLECTION_NAME)
                            .Find(new BsonDocument("_t", typeof(T).Name))
                            .ToList()
                            .ToAsyncEnumerable();

        public IAsyncEnumerable<IDomainEvent> GetAllEventsByEventType(Type eventType)
            => EventStoreManager.Client
                            .GetDatabase(Consts.CONST_DB_NAME)
                            .GetCollection<IDomainEvent>(Consts.CONST_EVENTS_COLLECTION_NAME)
                            .Find(new BsonDocument("_t", eventType.Name))
                            .ToList()
                            .ToAsyncEnumerable();

        public IAsyncEnumerable<IDomainEvent> GetAllEventsByAggregateId<TAggregateType, TAggregateId>(TAggregateId id)
            where TAggregateType : AggregateRoot<TAggregateId>
            => GetAllEventsByAggregateId(typeof(TAggregateType), id);

        public IAsyncEnumerable<IDomainEvent> GetAllEventsByAggregateId(Type aggregateType, object aggregateId)
        {
            var filterBuilder = Builders<IDomainEvent>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(nameof(IDomainEvent.AggregateId), aggregateId),
                filterBuilder.Eq(nameof(IDomainEvent.AggregateType), aggregateType));

            var collection = EventStoreManager.Client
                            .GetDatabase(Consts.CONST_DB_NAME)
                            .GetCollection<IDomainEvent>(Consts.CONST_EVENTS_COLLECTION_NAME);

            return collection.Find(filter).Sort(Builders<IDomainEvent>.Sort.Ascending(nameof(IDomainEvent.Sequence))).ToEnumerable().ToAsyncEnumerable();
        }

        public async Task<Result> StoreDomainEventRangeAsync(IEnumerable<IDomainEvent> events)
        {
            foreach (var evt in events)
            {
                await PrepareForEventInsertionAsync(evt);
            }

            var collection = await GetEventCollectionAsync<IDomainEvent>().ConfigureAwait(false);
            await collection.InsertManyAsync(events).ConfigureAwait(false);
            return Result.Ok();
        }

        public async Task<Result> StoreDomainEventAsync(IDomainEvent @event)
        {
            if (@event.GetType().IsDefined(typeof(EventNotPersistedAttribute)))
            {
                return Result.Ok();
            }
            await PrepareForEventInsertionAsync(@event);

            var collection = await GetEventCollectionAsync<IDomainEvent>().ConfigureAwait(false);
            await collection.InsertOneAsync(@event).ConfigureAwait(false);
            return Result.Ok();
        }

        #endregion

        #region IAggregateEventStore

        public async Task<IEventSourcedAggregate> GetRehydratedAggregateAsync<TId>(TId aggregateUniqueId, Type aggregateType)
        {
            if (aggregateType == null)
            {
                throw new ArgumentNullException(nameof(aggregateType));
            }

            var events = await GetAllEventsByAggregateId(aggregateType, aggregateUniqueId).ToList().ConfigureAwait(false);

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
            aggInstance.RehydrateState(events);

            return aggInstance;
        }

        public async Task<TAggregate> GetRehydratedAggregateAsync<TAggregate, TId>(TId aggregateUniqueId) where TAggregate : EventSourcedAggregate<TId>, new()
            => (await GetRehydratedAggregateAsync(aggregateUniqueId, typeof(TAggregate)).ConfigureAwait(false)) as TAggregate;

        #endregion

        #region Private methods

        private async Task PrepareForEventInsertionAsync(IDomainEvent @event)
        {
            var evtType = @event.GetType();
            IGenericSnapshotBehavior behavior = _snapshotBehaviorProvider?.GetBehaviorForEventType(evtType);
            CheckIdAndSetNewIfNeeded(@event);
            ulong? sequence = null;
            if (@event.AggregateId != null)
            {
                await SetSequenceAsync(@event, sequence).ConfigureAwait(false);
                if (_archiveBehavior != SnapshotEventsArchiveBehavior.Disabled
                    && behavior != null
                    && await behavior.IsSnapshotNeededAsync(@event.AggregateId, @event.AggregateType).ConfigureAwait(false))
                {
                    var aggregate = await GetRehydratedAggregateAsync(@event.AggregateId, @event.AggregateType).ConfigureAwait(false);
                    var result = await behavior.GenerateSnapshotAsync(@event.AggregateId, @event.AggregateType, aggregate).ConfigureAwait(false);
                    if (result.Snapshot != null)
                    {
                        await InsertSnapshotAsync(result.Snapshot).ConfigureAwait(false);
                    }
                    if (result.ArchiveEvents?.Any() == true)
                    {
                        await StoreArchiveEventsAsync(result.ArchiveEvents).ConfigureAwait(false);
                    }
                }
            }
        }

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
