using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.EventStore.CosmosDb
{
    internal class CosmosDbEventStore : DisposableObject, IEventStore
    {
        public Task<IEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TAggregate>(Guid aggregateUniqueId)
        {
            throw new NotImplementedException();
        }

        public Task StoreDomainEventAsync(IDomainEvent @event)
        {
            throw new NotImplementedException();
        }

        Task<TEvent> IEventStore.GetEventById<TEvent>(Guid eventId)
        {
            throw new NotImplementedException();
        }
    }
}
