using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.Attributes;
using CQELight.EventStore.CosmosDb.Common;
using CQELight.EventStore.CosmosDb.Models;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CQELight.EventStore.CosmosDb
{
    internal class CosmosDbEventStore : DisposableObject, IEventStore
    {
        internal EventStoreAzureDbContext _context;
        internal Uri _databaseLink;

        public CosmosDbEventStore(EventStoreAzureDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            InitDocumentDb().GetAwaiter().GetResult();
        }

        private async Task InitDocumentDb()
        {
            await _context.Client.CreateDatabaseIfNotExistsAsync(new Database { Id = EventStoreAzureDbContext.CONST_DB_NAME }).ConfigureAwait(false);
            await _context.Client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(EventStoreAzureDbContext.CONST_DB_NAME), new DocumentCollection { Id = EventStoreAzureDbContext.CONST_COLLECTION_NAME }).ConfigureAwait(false);
            _databaseLink = UriFactory.CreateDocumentCollectionUri(EventStoreAzureDbContext.CONST_DB_NAME, EventStoreAzureDbContext.CONST_COLLECTION_NAME);
        }

        public Task<IEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TAggregate>(Guid aggregateUniqueId)
        {
            return Task.Run(() => _context.Client.CreateDocumentQuery<Event>(_databaseLink).Where(@event => @event.AggregateId == aggregateUniqueId).ToList().Select(x => GetRehydratedEventFromDbEvent(x)).ToList().AsEnumerable());
        }   

        public async Task StoreDomainEventAsync(IDomainEvent @event)
        {
            if (@event.GetType().IsDefined(typeof(EventNotPersistedAttribute)))
            {
                return;
            }

            await SaveEvent(@event);
        }

        public Task<TEvent> GetEventById<TEvent>(Guid eventId)
            where TEvent : class, IDomainEvent
        {
            return Task.Run(() => GetRehydratedEventFromDbEvent(_context.Client.CreateDocumentQuery<Event>(_databaseLink).Where(@event => @event.Id == eventId).ToList().FirstOrDefault()) as TEvent);
        }

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
            return _context.Client.CreateDocumentAsync(_databaseLink, persistedEvent);
        }
    }
}
