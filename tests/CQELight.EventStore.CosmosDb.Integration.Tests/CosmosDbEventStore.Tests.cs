using CQELight.Abstractions;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.EventStore.CosmosDb.Common;
using FluentAssertions;
using Microsoft.Azure.Documents.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.EventStore.CosmosDb.Integration.Tests
{
    #region Nested classes

    class EventTest1 : IDomainEvent
    {
        public Guid Id { get; set; }

        public DateTime EventTime { get; set; }

        public Guid? AggregateId { get; set; }

        public Type AggregateType { get; set; }

        public ulong Sequence { get; set; }

        public string Texte { get; set; }
    }

    class EventTest2 : IDomainEvent
    {
        public Guid Id { get; set; }

        public DateTime EventTime { get; set; }

        public Guid? AggregateId { get; set; }

        public Type AggregateType { get; set; }

        public ulong Sequence { get; set; }

        public string Texte { get; set; }
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

    #endregion
    
    public class CosmosDbEventStoreTests
    {
        #region Variables

        private CosmosDbEventStore _cosmosDbEventStore;

        #endregion

        #region Constructeur

        public CosmosDbEventStoreTests()
        {
            EventStoreAzureDbContext.Activate(new AzureDbConfiguration("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="));
            _cosmosDbEventStore = new CosmosDbEventStore();
        }

        #endregion

        #region Tests  

        [Fact]
        private async Task InsertEventTest()
        {
            try
            {
                var eventToCreate = new EventTest1 { Id = Guid.NewGuid(), Texte = "toto", AggregateId = Guid.NewGuid(), EventTime = DateTime.Now };
                await _cosmosDbEventStore.StoreDomainEventAsync(eventToCreate).ConfigureAwait(false);


                var eventCreated = await _cosmosDbEventStore.GetEventById<EventTest1>(eventToCreate.Id);
                eventCreated.Should().NotBeNull();
                eventCreated.Id.Should().Be(eventToCreate.Id);
                eventCreated.Texte.Should().Be("toto");
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
                var eventTest = new EventTest1 { Id = id, AggregateId = Guid.NewGuid(), Texte = "toto", EventTime = DateTime.Now };
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest).ConfigureAwait(false);
                (await _cosmosDbEventStore.GetEventById<EventTest1>(id).ConfigureAwait(false)).Texte.Should().Be("toto");
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
                var id1 = Guid.NewGuid();
                var id2 = Guid.NewGuid();
                var id3 = Guid.NewGuid();
                var id4 = Guid.NewGuid();
                var idAggregate = Guid.NewGuid();
                var eventTest1 = new EventTest1 { Id = id1, Texte = "toto", AggregateId = idAggregate, EventTime = DateTime.Now };
                var eventTest2 = new EventTest2 { Id = id2, Texte = "tata", AggregateId = idAggregate, EventTime = DateTime.Now };
                var eventTest3 = new EventTest1 { Id = id3, Texte = "titi", AggregateId = idAggregate, EventTime = DateTime.Now };
                var eventTest4 = new EventTest2 { Id = id4, Texte = "tutu", AggregateId = idAggregate, EventTime = DateTime.Now };
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest1).ConfigureAwait(false);
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest2).ConfigureAwait(false);
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest3).ConfigureAwait(false);
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest4).ConfigureAwait(false);
                var results = await _cosmosDbEventStore.GetEventsFromAggregateIdAsync<SampleAgg>(idAggregate);

                var test1 = results.FirstOrDefault(x => x.Id == id1);
                test1.GetType().Should().Be(typeof(EventTest1));
                ((EventTest1)test1).Texte.Should().Be("toto");

                var test2 = results.FirstOrDefault(x => x.Id == id2);
                test2.GetType().Should().Be(typeof(EventTest2));
                ((EventTest2)test2).Texte.Should().Be("tata");

                var test3 = results.FirstOrDefault(x => x.Id == id3);
                test3.GetType().Should().Be(typeof(EventTest1));
                ((EventTest1)test3).Texte.Should().Be("titi");

                var test4 = results.FirstOrDefault(x => x.Id == id4);
                test4.GetType().Should().Be(typeof(EventTest2));
                ((EventTest2)test4).Texte.Should().Be("tutu");
            }
            finally
            {
                await DeleteAll();
            }
        }

        #endregion

        #region Méthodes privées

        private Task DeleteAll()
        {
            return EventStoreAzureDbContext.Client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(EventStoreAzureDbContext.CONST_DB_NAME));
        }

        #endregion        
    }
}
