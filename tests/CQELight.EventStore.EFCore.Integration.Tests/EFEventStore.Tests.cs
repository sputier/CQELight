using CQELight.Abstractions;
using CQELight.Abstractions.Events;
using CQELight.EventStore.Attributes;
using CQELight.EventStore.EFCore.Common;
using CQELight.EventStore.EFCore.Models;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.EventStore.EFCore.Integration.Tests
{
    public class EFEventStoreTests : BaseUnitTestClass
    {

        #region Ctor & members

        private static bool s_Init;

        public EFEventStoreTests()
        {
            if (!s_Init)
            {
                using (var ctx = GetContext())
                {
                    ctx.Database.EnsureDeleted();
                    ctx.Database.EnsureCreated();
                }
                s_Init = true;
            }
            new Bootstrapper().UseSQLServerWithEFCoreAsEventStore("Server=(localdb)\\mssqllocaldb;Database=Events_Tests_Base;Trusted_Connection=True;MultipleActiveResultSets=true;");
        }

        private EventStoreDbContext GetContext()
            => new EventStoreDbContext(new DbContextConfiguration
            {
                ConfigType = ConfigurationType.SQLServer,
                ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=Events_Tests_Base;Trusted_Connection=True;MultipleActiveResultSets=true;"
            });

        private void DeleteAll()
        {
            using (var ctx = GetContext())
            {
                ctx.RemoveRange(ctx.Set<Event>());
                ctx.SaveChanges();
            }
        }

        private async Task StoreTestEventAsync(Guid aggId, Guid id, DateTime date)
        {
            using (var store = new EFEventStore(GetContext()))
            {
                await store.StoreDomainEventAsync(new SampleEvent(aggId, id, date)
                {
                    Data = "testData"
                });
            }
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
        public async Task EFEventStore_StoreDomainEventAsync_NotPersisted()
        {
            try
            {
                using (var store = new EFEventStore(GetContext()))
                {
                    await store.StoreDomainEventAsync(new NotPersistedEvent());
                }

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count().Should().Be(0);
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFEventStore_StoreDomainEventAsync_AsExpected()
        {
            try
            {
                Guid aggId = Guid.NewGuid();
                Guid id = Guid.NewGuid();
                DateTime date = new DateTime(2018, 1, 1, 12, 00, 01);
                await StoreTestEventAsync(aggId, id, date);

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count().Should().Be(1);
                    var evt = ctx.Set<Event>().FirstOrDefault();
                    evt.Should().NotBeNull();
                    evt.AggregateId.Should().Be(aggId);
                    evt.Id.Should().Be(id);
                    evt.EventTime.Should().BeSameDateAs(date);
                    evt.Sequence.Should().Be(1);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        #endregion

        #region GetEventsFromAggregateIdAsync

        [Fact]
        public async Task EFEventStore_GetEventById_IdNotFound()
        {
            try
            {
                using (var store = new EFEventStore(GetContext()))
                {
                    (await store.GetEventById<SampleEvent>(Guid.NewGuid())).Should().BeNull();
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFEventStore_GetEventById_AsExpected()
        {
            try
            {
                Guid aggId = Guid.NewGuid();
                Guid id = Guid.NewGuid();
                DateTime date = new DateTime(2018, 1, 1, 12, 00, 01);
                await StoreTestEventAsync(aggId, id, date);

                using (var store = new EFEventStore(GetContext()))
                {
                    var evt = await store.GetEventById<SampleEvent>(id);
                    evt.Should().NotBeNull();
                    evt.AggregateId.Should().Be(aggId);
                    evt.Id.Should().Be(id);
                    evt.EventTime.Should().BeSameDateAs(date);
                    evt.Sequence.Should().Be(1);
                    evt.Data.Should().Be("testData");
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        #endregion

        #region GetEventsFromAggregateIdAsync

        [Fact]
        public async Task EFEventStore_GetEventsFromAggregateIdAsync_AsExpected()
        {
            try
            {
                var agg = new SampleAgg();
                agg.SimulateWork();
                await agg.DispatchDomainEvents();

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Should().HaveCount(2);
                }

                using (var store = new EFEventStore(GetContext()))
                {
                    var collection = await store.GetEventsFromAggregateIdAsync<SampleAgg>(agg.AggregateUniqueId);
                    collection.Should().HaveCount(2);

                    collection.Any(e => e.GetType() == typeof(AggCreated)).Should().BeTrue();
                    collection.Any(e => e.GetType() == typeof(AggDeleted)).Should().BeTrue();
                    collection.All(e => e.AggregateId == agg.AggregateUniqueId).Should().BeTrue();

                }
            }
            finally
            {
                DeleteAll();
            }
        }

        #endregion

    }
}
