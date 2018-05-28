using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.CosmosDb.Common;
using CQELight.Tools;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
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
            return GetCollection<IDomainEvent>(aggregateUniqueId);
        }

        private Task<IEnumerable<TEvent>> GetCollection<TEvent>(Guid aggregateUniqueId)
            where TEvent : class, IDomainEvent
        {
            return Task.Run(() => _context.Client.CreateDocumentQuery<TEvent>(_databaseLink).Where(@event => @event.AggregateId == aggregateUniqueId).ToList().AsEnumerable());
        }

        public async Task StoreDomainEventAsync(IDomainEvent @event)
        {
            await _context.Client.CreateDocumentAsync(_databaseLink, @event);
        }

        public Task<TEvent> GetEventById<TEvent>(Guid eventId)
            where TEvent : class, IDomainEvent
        {
            return Task.Run(() => _context.Client.CreateDocumentQuery<TEvent>(_databaseLink).Where(@event => @event.Id == eventId).ToList().FirstOrDefault());
        }
    }
}
