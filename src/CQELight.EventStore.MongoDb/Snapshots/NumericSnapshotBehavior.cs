using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.MongoDb.Models;
using CQELight.Tools.Extensions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.EventStore.MongoDb.Snapshots
{
    public class NumericSnapshotBehavior : ISnapshotBehavior
    {
        #region Members

        private readonly int _nbEvents;

        #endregion

        #region Ctor

        public NumericSnapshotBehavior(int nbEvents)
        {
            if (nbEvents < 2)
            {
                throw new ArgumentException("NumericSnapshotBehavior.ctor() : The number of events to generate" +
                    " a snapshot should be greater or equal to 2.");
            }
            _nbEvents = nbEvents;
        }

        #endregion

        #region ISnapshotBehavior methods

        public async Task<(ISnapshot Snapshot, int NewSequence)> GenerateSnapshotAsync(Guid aggregateId, Type aggregateType)
        {
            Snapshot snap = null;
            int newSequence = 1;
            if (aggregateType.CreateInstance() is IEventSourcedAggregate aggregateInstance)
            {
                var filterBuilder = Builders<IDomainEvent>.Filter;
                var filter = filterBuilder.And(
                    filterBuilder.Eq(nameof(IDomainEvent.AggregateId), aggregateId),
                    filterBuilder.Eq(nameof(IDomainEvent.AggregateType), nameof(aggregateType)));

                var collection = await GetEventCollectionAsync<IDomainEvent>().ConfigureAwait(false);

                var events = await collection.Find(filter).Sort(Builders<IDomainEvent>.Sort.Ascending(nameof(IDomainEvent.Sequence))).Limit(_nbEvents).ToListAsync().ConfigureAwait(false);

                aggregateInstance.RehydrateState(events);

                snap = new Snapshot(
                    aggregateId: aggregateId,
                    aggregateType: aggregateType.AssemblyQualifiedName,
                    aggregate: aggregateInstance,
                    snapshotBehaviorType: typeof(NumericSnapshotBehavior).AssemblyQualifiedName,
                    snapshotTime: DateTime.Now);              
            }
            return (snap, newSequence);
        }

        public async Task<bool> IsSnapshotNeededAsync(Guid aggregateId, Type aggregateType)
        {
            var filterBuilder = Builders<IDomainEvent>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(nameof(IDomainEvent.AggregateId), aggregateId),
                filterBuilder.Eq(nameof(IDomainEvent.AggregateType), nameof(aggregateType)));

            var collection = await GetEventCollectionAsync<IDomainEvent>().ConfigureAwait(false);

            return (await collection.Find(filter).CountDocumentsAsync().ConfigureAwait(false)) >= _nbEvents;
        }

        #endregion

        #region Private methods

        private async Task<IMongoCollection<T>> GetEventCollectionAsync<T>()
           where T : IDomainEvent
        {
            var collection = EventStoreManager.Client
                      .GetDatabase(Consts.CONST_DB_NAME)
                      .GetCollection<T>(Consts.CONST_COLLECTION_NAME);


            await collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<T>(Builders<T>.IndexKeys
                                                            .Ascending(nameof(IDomainEvent.AggregateId))
                                                            .Ascending(nameof(IDomainEvent.AggregateType))
                                                            .Ascending(nameof(IDomainEvent.Sequence)))
                ).ConfigureAwait(false);

            return collection;
        }      

        #endregion
    }
}
