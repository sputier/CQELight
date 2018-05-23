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
    public class CosmosDbEventStoreTests
    {
        [Fact]
        private async Task InsertEventTest()
        {
            var config = new EventStoreAzureDbContext(new AzureDbConfiguration {EndPointUrl = "https://localhost:8081", PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" });
            var cosmosEventStore = new CosmosDbEventStore(config);
            await cosmosEventStore.StoreDomainEventAsync(new EventTest { Id = Guid.NewGuid(), AggregateId = Guid.NewGuid(), EventTime = DateTime.Now }).ConfigureAwait(false);
        }

        [Fact]
        private async Task GetEventTest()
        {
            var config = new EventStoreAzureDbContext(new AzureDbConfiguration { EndPointUrl = "https://localhost:8081", PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" });
            var cosmosEventStore = new CosmosDbEventStore(config);
            var id = Guid.NewGuid();
            var eventTest = new EventTest { Id = id, AggregateId = Guid.NewGuid(), EventTime = DateTime.Now };
            await cosmosEventStore.StoreDomainEventAsync(eventTest).ConfigureAwait(false);
            (await cosmosEventStore.GetEventById<EventTest>(id).ConfigureAwait(false)).Should().BeSameAs(eventTest);
        }

        [Fact]
        private async Task GetEventsTest()
        {
            var config = new EventStoreAzureDbContext(new AzureDbConfiguration { EndPointUrl = "https://localhost:8081", PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" });
            var cosmosEventStore = new CosmosDbEventStore(config);
            var idAggregate = Guid.NewGuid();
            var eventTest1 = new EventTest { Id = Guid.NewGuid(), AggregateId = idAggregate, EventTime = DateTime.Now };
            var eventTest2 = new EventTest { Id = Guid.NewGuid(), AggregateId = idAggregate, EventTime = DateTime.Now };
            var eventTest3 = new EventTest { Id = Guid.NewGuid(), AggregateId = idAggregate, EventTime = DateTime.Now };
            var eventTest4 = new EventTest { Id = Guid.NewGuid(), AggregateId = idAggregate, EventTime = DateTime.Now };
            await cosmosEventStore.StoreDomainEventAsync(eventTest1).ConfigureAwait(false);
            await cosmosEventStore.StoreDomainEventAsync(eventTest2).ConfigureAwait(false);
            await cosmosEventStore.StoreDomainEventAsync(eventTest3).ConfigureAwait(false);
            await cosmosEventStore.StoreDomainEventAsync(eventTest4).ConfigureAwait(false);
            var results = await cosmosEventStore.GetEventsFromAggregateIdAsync<EventTest>(idAggregate);
        }
    }
}
