using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.GregYoungsEventStore;
using CQELight.EventStore.GregYoungsEventStore.Common;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CDELight.EventStore.GregYoungsEventStore
{
    public class GYEventStore : IEventStore, IAggregateEventStore
    {

        public GYEventStore(ISnapshotBehaviorProvider snapshotBehaviorProvider = null)
        {
            _snapshotBehaviorProvider = snapshotBehaviorProvider;
        }

        private ISnapshotBehaviorProvider _snapshotBehaviorProvider;

        #region IEventStore implementation

        public Task<IAsyncEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TAggregate, TId>(TId aggregateUniqueId) where TAggregate : EventSourcedAggregate<TId>
        {
            throw new NotImplementedException();
        }

        public Task<IAsyncEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TId>(TId aggregateUniqueId, Type aggregateType)
        {
            throw new NotImplementedException();
        }        

        public Task StoreDomainEventAsync(IDomainEvent @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var eventData = new EventData(@event.Id,@event.GetType().AssemblyQualifiedName, true ,Encoding.UTF8.GetBytes(@event.SerializeToJson()),Encoding.UTF8.GetBytes(@event.SerializeMetadataToJson()));
            return EventStoreManager.Connection.AppendToStreamAsync(@event.AggregateId.ToString(), ExpectedVersion.Any, new[] { eventData });                      
        }

        Task<TEvent> IEventStore.GetEventByIdAsync<TEvent>(Guid eventId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAggregateEventStore implementation


        public Task<IEventSourcedAggregate> GetRehydratedAggregateAsync<TId>(TId aggregateUniqueId, Type aggregateType)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregate> GetRehydratedAggregateAsync<TAggregate, TId>(TId aggregateUniqueId) where TAggregate : EventSourcedAggregate<TId>, new()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
