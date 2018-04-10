using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.Attributes;
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
    public class MongoDbEventStore : IEventStore
    {

        #region Private static members

        internal static bool s_indexesOk = false;

        #endregion

        #region Private methods

        private async Task<IMongoCollection<T>> GetCollectionAsync<T>()
            where T : IDomainEvent
        {
            var collection = EventStoreManager.Client
                      .GetDatabase(Consts.CONST_DB_NAME)
                      .GetCollection<T>(Consts.CONST_COLLECTION_NAME);
            

            await collection.Indexes.CreateOneAsync(
                    Builders<T>.IndexKeys
                    .Ascending(nameof(IDomainEvent.AggregateId))
                    .Ascending(nameof(IDomainEvent.AggregateType))
                    .Ascending(nameof(IDomainEvent.Sequence))
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
                var collection = await GetCollectionAsync<IDomainEvent>();
                currentSeq = await collection.CountAsync(filter).ConfigureAwait(false);
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

        #region IEventStore

        /// <summary>
        /// Get a collection of events for a specific aggregate.
        /// </summary>
        /// <param name="aggregateUniqueId">Id of the aggregate which we want all the events.</param>
        /// <typeparam name="TAggregate">Aggregate type.</typeparam>
        /// <returns>Collection of all associated events.</returns>
        public async Task<IEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TAggregate>(Guid aggregateUniqueId)
        {
            var filterBuilder = Builders<IDomainEvent>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(nameof(IDomainEvent.AggregateId), aggregateUniqueId),
                filterBuilder.Eq(nameof(IDomainEvent.AggregateType), typeof(TAggregate)));

            var collection = await GetCollectionAsync<IDomainEvent>();

            var result = await collection.Find(filter).Sort(Builders<IDomainEvent>.Sort.Ascending(nameof(IDomainEvent.Sequence))).ToListAsync().ConfigureAwait(false);

            return result.AsEnumerable();
        }

        /// <summary>
        /// Store a domain event in the event store
        /// </summary>
        /// <param name="event">Event instance to be persisted.</param>
        public async Task StoreDomainEventAsync(IDomainEvent @event)
        {
            if (@event.GetType().IsDefined(typeof(EventNotPersistedAttribute)))
            {
                return;
            }

            CheckIdAndSetNewIfNeeded(@event);
            await SetSequenceAsync(@event).ConfigureAwait(false);

            var collection = await GetCollectionAsync<IDomainEvent>();
            await collection.InsertOneAsync(@event).ConfigureAwait(false);
        }

        /// <summary>
        /// Get an event per its id.
        /// </summary>
        /// <param name="eventId">Id of the event.</param
        /// <typeparam name="TEvent">Type of event to retrieve.</typeparam>
        /// <returns>Instance of the event.</returns>
        public async Task<TEvent> GetEventById<TEvent>(Guid eventId)
            where TEvent : class, IDomainEvent
        {
            var filter = Builders<TEvent>.Filter.Eq(e => e.Id, eventId);
            var collection = await GetCollectionAsync<TEvent>();
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        #endregion

    }
}
