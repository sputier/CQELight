using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.MongoDb.Models;
using CQELight.Extensions;
using CQELight.Tools.Extensions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public async Task<(ISnapshot Snapshot, IEnumerable<IDomainEvent> ArchiveEvents)>
            GenerateSnapshotAsync(object aggregateId, Type aggregateType, IEventSourcedAggregate rehydratedAggregate)
        {
            Snapshot snap = null;
            List<IDomainEvent> events = new List<IDomainEvent>();
            var filterBuilder = Builders<IDomainEvent>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(nameof(IDomainEvent.AggregateId), aggregateId),
                filterBuilder.Eq(nameof(IDomainEvent.AggregateType), aggregateType.AssemblyQualifiedName));


            var collection = GetEventCollection<IDomainEvent>();

            events = await collection.Find(filter).Sort(Builders<IDomainEvent>.Sort.Ascending(nameof(IDomainEvent.Sequence)))
                .Limit(_nbEvents).ToListAsync().ConfigureAwait(false);

            PropertyInfo stateProp = aggregateType.GetAllProperties().FirstOrDefault(p => p.PropertyType.IsSubclassOf(typeof(AggregateState)));
            FieldInfo stateField = aggregateType.GetAllFields().FirstOrDefault(f => f.FieldType.IsSubclassOf(typeof(AggregateState)));
            Type stateType = stateProp?.PropertyType ?? stateField?.FieldType;

            AggregateState state = null;
            if (stateProp != null)
            {
                state = stateProp.GetValue(rehydratedAggregate) as AggregateState;
            }
            else
            {
                state = stateField.GetValue(rehydratedAggregate) as AggregateState;
            }

            if (state == null)
            {
                throw new InvalidOperationException("MongoDbEventStore.GetRehydratedAggregateAsync() : Cannot find property/field that manage state for aggregate" +
                        $" type {aggregateType.FullName}. State should be a property or a field of the aggregate");
            }

            snap = new Snapshot(
              aggregateId: aggregateId,
              aggregateType: aggregateType.AssemblyQualifiedName,
              aggregateState: state,
              snapshotBehaviorType: typeof(NumericSnapshotBehavior).AssemblyQualifiedName,
              snapshotTime: DateTime.Now);

            return (snap, events);
        }

        public async Task<bool> IsSnapshotNeededAsync(object aggregateId, Type aggregateType)
        {
            var filterBuilder = Builders<IDomainEvent>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(nameof(IDomainEvent.AggregateId), aggregateId),
                filterBuilder.Eq(nameof(IDomainEvent.AggregateType), aggregateType.AssemblyQualifiedName));

            var collection = GetEventCollection<IDomainEvent>();

            return (await collection.CountDocumentsAsync(filter).ConfigureAwait(false)) >= _nbEvents;
        }

        #endregion

        #region Private methods

        private IMongoCollection<T> GetEventCollection<T>()
           where T : IDomainEvent => EventStoreManager.Client
                      .GetDatabase(Consts.CONST_DB_NAME)
                      .GetCollection<T>(Consts.CONST_EVENTS_COLLECTION_NAME);

        #endregion

    }
}
