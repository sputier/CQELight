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
    internal class CosmosDbEventStore : IEventStore
    {

        #region IEventStore methods

        public Task<IEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TAggregate>(Guid aggregateUniqueId) 
            => Task.Run(() => EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.DatabaseLink)
                             .Where(@event => @event.AggregateId == aggregateUniqueId).ToList().Select(x => GetRehydratedEventFromDbEvent(x)).ToList().AsEnumerable());

        public Task StoreDomainEventAsync(IDomainEvent @event)
        {
            if (@event.GetType().IsDefined(typeof(EventNotPersistedAttribute)))
            {
                return Task.CompletedTask;
            }

            return SaveEvent(@event);
        }

        public Task<TEvent> GetEventById<TEvent>(Guid eventId)
            where TEvent : class, IDomainEvent 
            => Task.Run(() 
                => GetRehydratedEventFromDbEvent(EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.DatabaseLink)
                                                                                                 .Where(@event => @event.Id == eventId).ToList().FirstOrDefault()) as TEvent);

        #endregion        

        #region Private methods

        private IDomainEvent GetRehydratedEventFromDbEvent(Event evt)
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt));
            }

            var evtType = Type.GetType(evt.EventType);
            var rehydratedEvt = evt.EventData.FromJson(evtType) as IDomainEvent;
            var properties = evtType.GetAllProperties();

            properties.First(p => p.Name == nameof(IDomainEvent.AggregateId)).SetMethod?.Invoke(rehydratedEvt, new object[] { evt.AggregateId });
            properties.First(p => p.Name == nameof(IDomainEvent.Id)).SetMethod?.Invoke(rehydratedEvt, new object[] { evt.Id });
            properties.First(p => p.Name == nameof(IDomainEvent.EventTime)).SetMethod?.Invoke(rehydratedEvt, new object[] { evt.EventTime });
            properties.First(p => p.Name == nameof(IDomainEvent.Sequence)).SetMethod?.Invoke(rehydratedEvt, new object[] { Convert.ToUInt64(evt.Sequence) });
            return rehydratedEvt;
        }

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
