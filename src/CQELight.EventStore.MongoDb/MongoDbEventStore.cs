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

        #region Private methods

        private IMongoCollection<T> GetCollection<T>()
            where T : IDomainEvent
            => EventStoreManager.Client
                    .GetDatabase(Consts.CONST_DB_NAME)
                    .GetCollection<T>(Consts.CONST_COLLECTION_NAME);

        private async Task SetSequenceAsync(IDomainEvent @event)
        {
            long currentSeq = 0;
            var sequenceProp = @event.GetType().GetAllProperties().FirstOrDefault(m => m.Name == nameof(IDomainEvent.Sequence));
            if (@event.AggregateId.HasValue && sequenceProp?.SetMethod != null)
            {
                var filter = Builders<IDomainEvent>.Filter.Eq(nameof(IDomainEvent.AggregateId), @event.AggregateId.Value);
                currentSeq = await GetCollection<IDomainEvent>().CountAsync(filter).ConfigureAwait(false);
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
            var result = await GetCollection<IDomainEvent>()
                    .Find(filter)
                    .ToListAsync().ConfigureAwait(false);
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

            await GetCollection<IDomainEvent>().InsertOneAsync(@event).ConfigureAwait(false);
        }

        /// <summary>
        /// Get an event per its id.
        /// </summary>
        /// <param name="eventId">Id of the event.</param
        /// <typeparam name="TEvent">Type of event to retrieve.</typeparam>
        /// <returns>Instance of the event.</returns>
        public Task<TEvent> GetEventById<TEvent>(Guid eventId)
            where TEvent : class, IDomainEvent
        {
            var filter = Builders<TEvent>.Filter.Eq(e => e.Id, eventId);
            return GetCollection<TEvent>()
                .Find(filter)
                .FirstOrDefaultAsync();
        }

        #endregion

    }
}
