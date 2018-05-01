using CQELight.Abstractions;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Bootstrapping.Notifications;
using CQELight.EventStore.Attributes;
using CQELight.TestFramework;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.EventStore.MongoDb.Integration.Tests
{
    public class MongoDbEventStoreTests : BaseUnitTestClass
    {

        #region Ctor & members

        private static bool s_Init;

        public MongoDbEventStoreTests()
        {
            if (!s_Init)
            {
                var c = new ConfigurationBuilder().AddJsonFile("test-config.json").Build();
                new Bootstrapper()
                    .UseMongoDbAsEventStore($"mongodb://{c["host"]}:{c["port"]}")
                    .Bootstrapp(out List<BootstrapperNotification> notifs);
                s_Init = true;
            }
        }

        private void DeleteAll()
        {
            EventStoreManager.Client.DropDatabase(Consts.CONST_DB_NAME);
        }

        private IMongoCollection<IDomainEvent> GetCollection()
            => EventStoreManager.Client.GetDatabase(Consts.CONST_DB_NAME).GetCollection<IDomainEvent>(Consts.CONST_COLLECTION_NAME);

        private async Task StoreTestEventAsync(Guid aggId, Guid id, DateTime date)
        {
            var store = new MongoDbEventStore();
            await store.StoreDomainEventAsync(new SampleEvent(aggId, id, date)
            {
                Data = "testData"
            }).ConfigureAwait(false);
        }

        #region Nested class

        [EventNotPersisted]
        private class NotPersistedEvent : BaseDomainEvent
        {

        }

        private class SampleEvent : BaseDomainEvent
        {
            public string Data { get; set; }

            public SampleEvent(Guid aggId, Guid evtId, DateTime date)
            {
                AggregateId = aggId;
                Id = evtId;
                EventTime = date;
            }
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

        #endregion

        #region StoreDomainEventAsync

        [Fact]
        public async Task MongoDbEventStoreStoreDomainEventAsync_NotPersisted()
        {
            try
            {
                var store = new MongoDbEventStore();
                await store.StoreDomainEventAsync(new NotPersistedEvent()).ConfigureAwait(false);
                (await GetCollection().CountAsync(FilterDefinition<IDomainEvent>.Empty).ConfigureAwait(false)).Should().Be(0);
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task MongoDbEventStoreStoreDomainEventAsync_AsExpected()
        {
            try
            {
                Guid aggId = Guid.NewGuid();
                Guid id = Guid.NewGuid();
                DateTime date = new DateTime(2018, 1, 1, 12, 00, 01);
                await StoreTestEventAsync(aggId, id, date).ConfigureAwait(false);

                (await GetCollection().CountAsync(FilterDefinition<IDomainEvent>.Empty).ConfigureAwait(false)).Should().Be(1);
                var evt = await GetCollection().Find(FilterDefinition<IDomainEvent>.Empty).FirstOrDefaultAsync().ConfigureAwait(false);
                evt.Should().NotBeNull();
                evt.AggregateId.Should().Be(aggId);
                evt.Id.Should().Be(id);
                evt.EventTime.Should().BeSameDateAs(date);
                evt.Sequence.Should().Be(1);
            }
            finally
            {
                DeleteAll();
            }
        }

        #endregion

        #region GetEventsFromAggregateIdAsync

        [Fact]
        public async Task MongoDbEventStoreGetEventById_IdNotFound()
        {
            try
            {
                var store = new MongoDbEventStore();
                (await store.GetEventById<SampleEvent>(Guid.NewGuid()).ConfigureAwait(false)).Should().BeNull();
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task MongoDbEventStoreGetEventById_AsExpected()
        {
            try
            {
                Guid aggId = Guid.NewGuid();
                Guid id = Guid.NewGuid();
                DateTime date = new DateTime(2018, 1, 1, 12, 00, 01);
                await StoreTestEventAsync(aggId, id, date).ConfigureAwait(false);

                var store = new MongoDbEventStore();
                var evt = await store.GetEventById<SampleEvent>(id).ConfigureAwait(false);
                evt.Should().NotBeNull();
                evt.AggregateId.Should().Be(aggId);
                evt.Id.Should().Be(id);
                evt.EventTime.Should().BeSameDateAs(date);
                evt.Sequence.Should().Be(1);
                evt.Data.Should().Be("testData");
            }
            finally
            {
                DeleteAll();
            }
        }

        #endregion

        #region GetEventsFromAggregateIdAsync

        [Fact]
        public async Task MongoDbEventStoreGetEventsFromAggregateIdAsync_AsExpected()
        {
            try
            {
                DeleteAll();
                var agg = new SampleAgg();
                agg.SimulateWork();
                await agg.DispatchDomainEvents().ConfigureAwait(false);

                (await GetCollection().CountAsync(FilterDefinition<IDomainEvent>.Empty).ConfigureAwait(false)).Should().Be(2);

                var store = new MongoDbEventStore();
                var collection = await store.GetEventsFromAggregateIdAsync<SampleAgg>(agg.AggregateUniqueId).ConfigureAwait(false);
                collection.Should().HaveCount(2);

                collection.Any(e => e.GetType() == typeof(AggCreated)).Should().BeTrue();
                collection.Any(e => e.GetType() == typeof(AggDeleted)).Should().BeTrue();
                collection.All(e => e.AggregateId == agg.AggregateUniqueId).Should().BeTrue();

                collection.First().Should().BeOfType<AggCreated>();
                collection.Skip(1).First().Should().BeOfType<AggDeleted>();
            }
            finally
            {
                DeleteAll();
            }
        }

        #endregion

    }
}
