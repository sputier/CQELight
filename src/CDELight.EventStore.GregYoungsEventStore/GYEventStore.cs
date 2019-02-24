using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.GregYoungsEventStore;
using CQELight.EventStore.GregYoungsEventStore.Common;
using CQELight.Tools.Extensions;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        => GetEventsFromAggregateIdAsync(aggregateUniqueId, typeof(TAggregate));
    
        public async Task<IAsyncEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TId>(TId aggregateUniqueId, Type aggregateType)
        {
            var events = new List<IDomainEvent>();
            var index = 0;
            bool allEventsRead = false;
            while (allEventsRead)
            {
                var result = await EventStoreManager.Connection.ReadStreamEventsForwardAsync(aggregateUniqueId.ToString(), index, 1000, false).ConfigureAwait(false);
                if (result.Events.Length == 0)
                {
                    allEventsRead = true;
                }
                events.AddRange(result.Events.Select(x => GetRehydratedEventFromDbEvent(x.Event)));
                index = 1000;
            }

            return events.ToAsyncEnumerable();
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


        public async Task<IEventSourcedAggregate> GetRehydratedAggregateAsync<TId>(TId aggregateUniqueId, Type aggregateType)
        {
            var events = await (await GetEventsFromAggregateIdAsync(aggregateUniqueId, aggregateType).ConfigureAwait(false)).ToList().ConfigureAwait(false);

            if (!(aggregateType.CreateInstance() is IEventSourcedAggregate aggregate))
            {
                throw new InvalidOperationException("EFEventStore.GetRehydratedAggregateAsync() : Cannot create a new instance of" +
                    $" {aggregateType.FullName} aggregate. It should have one parameterless constructor (can be private).");
            }

            aggregate.RehydrateState(events);
            return aggregate;
        }

        public async Task<TAggregate> GetRehydratedAggregateAsync<TAggregate, TId>(TId aggregateUniqueId) where TAggregate : EventSourcedAggregate<TId>, new()
        => (await GetRehydratedAggregateAsync(aggregateUniqueId, typeof(TAggregate)).ConfigureAwait(false)) as TAggregate;

        #endregion


        #region privates methods

        private IDomainEvent GetRehydratedEventFromDbEvent(RecordedEvent evt)
        {
            var evtType = Type.GetType(evt.EventType);
            return Encoding.UTF8.GetString(evt.Data).FromJson(evtType) as IDomainEvent;
        }

        #endregion
    }
}
