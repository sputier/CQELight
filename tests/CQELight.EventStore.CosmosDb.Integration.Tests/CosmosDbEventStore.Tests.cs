using CQELight.Abstractions;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.EventStore.CosmosDb.Common;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.EventStore.CosmosDb.Integration.Tests
{
    class EventTest : IDomainEvent
    {
        public Guid Id { get; set; }

        public DateTime EventTime { get; set; }

        public Guid? AggregateId { get; set; }

        public Type AggregateType { get; set; }

        public ulong Sequence { get; set; }
    }

    public class SampleAgg : AggregateRoot<Guid>
    {
        public void SimulateWork()
        {
            AddDomainEvent(new AggCreated());
            AddDomainEvent(new AggDeleted());
        }

    }

    public class AggCreated : BaseDomainEvent
    {
    }
    public class AggDeleted : BaseDomainEvent
    {
    }

    public class CosmosDbEventStoreTests
    {
        private CosmosDbEventStore _cosmosDbEventStore;

        public CosmosDbEventStoreTests()
        {
            var config = new EventStoreAzureDbContext(new AzureDbConfiguration { EndPointUrl = "https://localhost:8081", PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" });
            _cosmosDbEventStore = new CosmosDbEventStore(config);
        }

        [Fact]
        private async Task InsertEventTest()
        {
            try
            { 
                await _cosmosDbEventStore.StoreDomainEventAsync(new EventTest { Id = Guid.NewGuid(), AggregateId = Guid.NewGuid(), EventTime = DateTime.Now }).ConfigureAwait(false);
            }
            finally
            {
                await DeleteAll();
            }
        }

        [Fact]
        private async Task GetEventTest()
        {
            try
            { 
                var id = Guid.NewGuid();
                var eventTest = new EventTest { Id = id, AggregateId = Guid.NewGuid(), EventTime = DateTime.Now };
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest).ConfigureAwait(false);
                (await _cosmosDbEventStore.GetEventById<EventTest>(id).ConfigureAwait(false)).Should().BeSameAs(eventTest);
            }
            finally
            {
                await DeleteAll();
            }
        }

        [Fact]
        private async Task GetEventsTest()
        {
            try
            {
                var idAggregate = Guid.NewGuid();
                var eventTest1 = new EventTest { Id = Guid.NewGuid(), AggregateId = idAggregate, EventTime = DateTime.Now };
                var eventTest2 = new EventTest { Id = Guid.NewGuid(), AggregateId = idAggregate, EventTime = DateTime.Now };
                var eventTest3 = new EventTest { Id = Guid.NewGuid(), AggregateId = idAggregate, EventTime = DateTime.Now };
                var eventTest4 = new EventTest { Id = Guid.NewGuid(), AggregateId = idAggregate, EventTime = DateTime.Now };
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest1).ConfigureAwait(false);
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest2).ConfigureAwait(false);
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest3).ConfigureAwait(false);
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest4).ConfigureAwait(false);
                var results = await _cosmosDbEventStore.GetEventsFromAggregateIdAsync<SampleAgg>(idAggregate);
            }
            finally
            {
                await DeleteAll();
            }
        }

        private async Task DeleteAll()
        {
            await _cosmosDbEventStore._context.Client.DeleteDatabaseAsync(_cosmosDbEventStore._databaseLink);
        }
    }
}
