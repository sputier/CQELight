using CQELight.Abstractions;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.CosmosDb.Common;
using CQELight.EventStore.CosmosDb.Models;
using CQELight.EventStore.CosmosDb.Snapshots;
using CQELight.Tools.Extensions;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Moq;
using System;
using System.Collections.Generic;
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
        #region Ctor & members

        private CosmosDbEventStore _cosmosDbEventStore;
        private Mock<ISnapshotBehaviorProvider> _snapshotProviderMock;

        public CosmosDbEventStoreTests()
        {
            _snapshotProviderMock = new Mock<ISnapshotBehaviorProvider>();
            EventStoreAzureDbContext.Activate(new AzureDbConfiguration("https://localhost:8081",
                "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="))
                .GetAwaiter().GetResult();
            _cosmosDbEventStore = GetCosmosDbEventStore();
        }

        private async Task DeleteAllAsync()
        {
            var eventFeedResponse = await EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.EventsDatabaseLink)
                .AsDocumentQuery().ExecuteNextAsync<Document>().ConfigureAwait(false);
            await eventFeedResponse.DoForEachAsync(async e => await EventStoreAzureDbContext.Client.DeleteDocumentAsync(documentLink: e.SelfLink).ConfigureAwait(false))
                    .ConfigureAwait(false);
            
            var snapshotFeedResponse = await EventStoreAzureDbContext.Client.CreateDocumentQuery<Snapshot>(EventStoreAzureDbContext.SnapshotDatabaseLink)
                .AsDocumentQuery().ExecuteNextAsync<Document>().ConfigureAwait(false);
            await snapshotFeedResponse.DoForEachAsync(async e => await EventStoreAzureDbContext.Client.DeleteDocumentAsync(documentLink: e.SelfLink).ConfigureAwait(false))
                    .ConfigureAwait(false);
        }

        private CosmosDbEventStore GetCosmosDbEventStore(ISnapshotBehaviorProvider snapshotBehavior = null)
            => new CosmosDbEventStore(snapshotBehavior);
        #endregion

        #region GetEventById  

        [Fact]
        public async Task CosmosDbEventStore_GetEventById_AsExpected()
        {
            try
            {
                var eventToCreate = new EventTest1
                {
                    Id = Guid.NewGuid(),
                    Texte = "toto",
                    AggregateId = Guid.NewGuid(),
                    AggregateType = typeof(SampleAgg),
                    EventTime = DateTime.Now
                };
                await _cosmosDbEventStore.StoreDomainEventAsync(eventToCreate).ConfigureAwait(false);

                var eventCreated = await _cosmosDbEventStore.GetEventByIdAsync<EventTest1>(eventToCreate.Id).ConfigureAwait(false);
                eventCreated.Should().NotBeNull();
                eventCreated.Id.Should().Be(eventToCreate.Id);
                eventCreated.Texte.Should().Be("toto");
            }
            finally
            {
                await DeleteAllAsync().ConfigureAwait(false);
            }
        }

        #endregion

        #region StoreDomainEventAsync

        [Fact]
        public async Task CosmosDbEventStore_StoreDomainEventAsync_AsExpected()
        {
            try
            {
                var id = Guid.NewGuid();
                var eventTest = new EventTest1 { Id = id, AggregateId = Guid.NewGuid(), AggregateType = typeof(SampleAgg), Texte = "toto", EventTime = DateTime.Now };
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest).ConfigureAwait(false);
                (await _cosmosDbEventStore.GetEventByIdAsync<EventTest1>(id).ConfigureAwait(false)).Texte.Should().Be("toto");
            }
            finally
            {
                await DeleteAllAsync().ConfigureAwait(false);
            }
        }

        #endregion

        #region GetEventsFromAggregateIdAsync

        [Fact]
        public async Task CosmosDbEventStore_GetEventsFromAggregateIdAsync_AsExpected()
        {
            try
            {
                var id1 = Guid.NewGuid();
                var id2 = Guid.NewGuid();
                var id3 = Guid.NewGuid();
                var id4 = Guid.NewGuid();
                var idAggregate = Guid.NewGuid();
                var eventTest1 = new EventTest1 { Id = id1, Texte = "toto", AggregateId = idAggregate, AggregateType = typeof(SampleAgg), EventTime = DateTime.Now };
                var eventTest2 = new EventTest2 { Id = id2, Texte = "tata", AggregateId = idAggregate, AggregateType = typeof(SampleAgg), EventTime = DateTime.Now };
                var eventTest3 = new EventTest1 { Id = id3, Texte = "titi", AggregateId = idAggregate, AggregateType = typeof(SampleAgg), EventTime = DateTime.Now };
                var eventTest4 = new EventTest2 { Id = id4, Texte = "tutu", AggregateId = idAggregate, AggregateType = typeof(SampleAgg), EventTime = DateTime.Now };
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest1).ConfigureAwait(false);
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest2).ConfigureAwait(false);
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest3).ConfigureAwait(false);
                await _cosmosDbEventStore.StoreDomainEventAsync(eventTest4).ConfigureAwait(false);
                var results = await (await _cosmosDbEventStore.GetEventsFromAggregateIdAsync<SampleAgg>(idAggregate).ConfigureAwait(false)).ToList().ConfigureAwait(false);

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
                await DeleteAllAsync().ConfigureAwait(false);
            }
        }

        #endregion

        #region Snapshot behavior

        private class AggregateSnapshotEvent : BaseDomainEvent
        {
            private AggregateSnapshotEvent()
            {

            }
            public AggregateSnapshotEvent(Guid aggregateId)
            {
                AggregateId = aggregateId;
                AggregateType = typeof(AggregateSnapshot);
                Id = Guid.NewGuid();
            }
        }

        private class AggregateSnapshot : EventSourcedAggregate<Guid>
        {
            protected override AggregateState State
            {
                get => _state;
                set
                {
                    if (value is AggregateSnapshotState newState)
                    {
                        _state = newState;
                    }
                }
            }
            private AggregateSnapshotState _state = new AggregateSnapshotState();
            public int AggIncValue => _state.Increment;

            private class AggregateSnapshotState : AggregateState
            {
                public int Increment { get; private set; }

                public AggregateSnapshotState()
                {
                    AddHandler<AggregateSnapshotEvent>(AggregateSnapshotEventWhen);
                }

                private void AggregateSnapshotEventWhen(AggregateSnapshotEvent obj)
                    => Increment++;
            }

            public override void RehydrateState(IEnumerable<IDomainEvent> events)
                => _state.ApplyRange(events);
        }

        [Fact]
        public async Task CosmosDbEventStore_StoreDomainEventAsync_CreateSnapshot()
        {
            _snapshotProviderMock.Setup(m => m.GetBehaviorForEventType(typeof(AggregateSnapshotEvent))).Returns(new NumericSnapshotBehavior(10));
            try
            {
                var cosmosDbEventStore = GetCosmosDbEventStore(_snapshotProviderMock.Object);
                Guid aggId = Guid.NewGuid();
                for (int i = 0; i < 11; i++)
                {
                    await cosmosDbEventStore.StoreDomainEventAsync(new AggregateSnapshotEvent(aggId)).ConfigureAwait(false);
                }

                var events = EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.EventsDatabaseLink).ToList();
                events.Count.Should().Be(1);
                var evt = events.FirstOrDefault();
                evt.Should().NotBeNull();
                evt.Sequence.Should().Be(1);
                evt.EventData.Should().NotBeNullOrWhiteSpace();

                var snapshots = EventStoreAzureDbContext.Client.CreateDocumentQuery<Snapshot>(EventStoreAzureDbContext.SnapshotDatabaseLink).ToList();
                snapshots.Count.Should().Be(1);
                var snap = snapshots.FirstOrDefault();
                snap.Should().NotBeNull();
                snap.AggregateId.Should().Be(aggId);
                snap.AggregateType.Should().Be(typeof(AggregateSnapshot).AssemblyQualifiedName);

                var agg = await cosmosDbEventStore.GetRehydratedAggregateAsync<AggregateSnapshot>(aggId).ConfigureAwait(false);
                agg.Should().NotBeNull();
                agg.AggIncValue.Should().Be(11);
            }
            finally
            {
                await DeleteAllAsync();
            }
        }

        [Fact]
        public async Task CosmosDbEventStore_StoreDomainEventAsync_CreateSnapshot_Multiple_Same_Aggregates()
        {
            _snapshotProviderMock.Setup(m => m.GetBehaviorForEventType(typeof(AggregateSnapshotEvent))).Returns(new NumericSnapshotBehavior(10));
            try
            {
                var cosmosDbEventStore = GetCosmosDbEventStore(_snapshotProviderMock.Object);
                Guid aggId = Guid.NewGuid();
                for (int i = 0; i < 11; i++)
                {
                    await cosmosDbEventStore.StoreDomainEventAsync(new AggregateSnapshotEvent(aggId)).ConfigureAwait(false);
                }
                var otherId = Guid.NewGuid();
                for (int i = 0; i < 30; i++)
                {
                    if (i % 10 == 0)
                    {
                        otherId = Guid.NewGuid();
                    }
                    await cosmosDbEventStore.StoreDomainEventAsync(new AggregateSnapshotEvent(otherId)).ConfigureAwait(false);
                }

                var events = EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.EventsDatabaseLink).ToList();
                events.Count(e => e.AggregateId == aggId).Should().Be(1);
                var evt = events.Where(e => e.AggregateId == aggId).FirstOrDefault();
                evt.Should().NotBeNull();
                evt.AggregateId.Should().Be(aggId);
                evt.Sequence.Should().Be(1);
                evt.EventData.Should().NotBeNullOrWhiteSpace();

                var snapshots = EventStoreAzureDbContext.Client.CreateDocumentQuery<Snapshot>(EventStoreAzureDbContext.SnapshotDatabaseLink).ToList();
                snapshots.Count.Should().Be(1);
                var snap = snapshots.FirstOrDefault();
                snap.Should().NotBeNull();
                snap.AggregateId.Should().Be(aggId);
                snap.AggregateType.Should().Be(typeof(AggregateSnapshot).AssemblyQualifiedName);

                var agg = await cosmosDbEventStore.GetRehydratedAggregateAsync<AggregateSnapshot>(aggId).ConfigureAwait(false);
                agg.Should().NotBeNull();
                agg.AggIncValue.Should().Be(11);


                var agg2 = await cosmosDbEventStore.GetRehydratedAggregateAsync<AggregateSnapshot>(otherId).ConfigureAwait(false);
                agg2.Should().NotBeNull();
                agg2.AggIncValue.Should().Be(11);
            }
            finally
            {
                await DeleteAllAsync();
            }
        }

        [Fact]
        public async Task CosmosDbEventStore_StoreDomainEventAsync_NoSnapshotBehaviorDefined()
        {
            try
            {
                Guid aggId = Guid.NewGuid();
                for (int i = 0; i < 11; i++)
                {
                    await _cosmosDbEventStore.StoreDomainEventAsync(new AggregateSnapshotEvent(aggId)).ConfigureAwait(false);
                }

                var events = EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.EventsDatabaseLink).ToList();
                events.Count.Should().Be(11);

                var snapshots = EventStoreAzureDbContext.Client.CreateDocumentQuery<Snapshot>(EventStoreAzureDbContext.SnapshotDatabaseLink).ToList();
                snapshots.Count.Should().Be(0);

                var agg = await _cosmosDbEventStore.GetRehydratedAggregateAsync<AggregateSnapshot>(aggId).ConfigureAwait(false);
                agg.Should().NotBeNull();
                agg.AggIncValue.Should().Be(11);
            }
            finally
            {
                await DeleteAllAsync();
            }
        }
        #endregion

    }
}
