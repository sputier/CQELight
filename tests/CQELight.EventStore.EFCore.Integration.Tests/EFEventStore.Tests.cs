using CQELight.Abstractions;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Bootstrapping.Notifications;
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
using CQELight.Tools.Extensions;
using Xunit;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.EFCore.Snapshots;
using CQELight.Abstractions.EventStore;

namespace CQELight.EventStore.EFCore.Integration.Tests
{
    public class EFEventStoreTests : BaseUnitTestClass
    {
        #region Ctor & members

        private static bool s_Init;

        public EFEventStoreTests()
        {
            EventStoreManager.Behaviors = new Dictionary<Type, ISnapshotBehavior>();
            if (!s_Init)
            {
                using (var ctx = GetContext())
                {
                    ctx.Database.EnsureDeleted();
                    ctx.Database.EnsureCreated();
                }
                s_Init = true;
            }
            DeleteAll();
            new Bootstrapper()
                .UseSQLServerWithEFCoreAsEventStore("Server=(localdb)\\mssqllocaldb;Database=Events_Tests_Base;Trusted_Connection=True;MultipleActiveResultSets=true;")
                .Bootstrapp();
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
                ctx.RemoveRange(ctx.Set<Snapshot>());
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
                }).ConfigureAwait(false);
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

        private class SampleAggEvent : BaseDomainEvent
        {
            public string Data { get; set; }

            public SampleAggEvent(Guid aggId, Guid evtId, DateTime date)
            {
                AggregateType = typeof(SampleAgg);
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
                    await store.StoreDomainEventAsync(new NotPersistedEvent()).ConfigureAwait(false);
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
                await StoreTestEventAsync(aggId, id, date).ConfigureAwait(false);

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

        #region GetEventByIdAsync

        [Fact]
        public async Task EFEventStore_GetEventByIdAsync_IdNotFound()
        {
            try
            {
                using (var store = new EFEventStore(GetContext()))
                {
                    (await store.GetEventByIdAsync<SampleEvent>(Guid.NewGuid()).ConfigureAwait(false)).Should().BeNull();
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFEventStore_GetEventByIdAsync_AsExpected()
        {
            try
            {
                Guid aggId = Guid.NewGuid();
                Guid id = Guid.NewGuid();
                DateTime date = new DateTime(2018, 1, 1, 12, 00, 01);
                await StoreTestEventAsync(aggId, id, date).ConfigureAwait(false);

                using (var store = new EFEventStore(GetContext()))
                {
                    var evt = await store.GetEventByIdAsync<SampleEvent>(id).ConfigureAwait(false);
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
                DeleteAll();
                var agg = new SampleAgg();
                agg.SimulateWork();
                await agg.DispatchDomainEventsAsync().ConfigureAwait(false);

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Should().HaveCount(2);
                }

                using (var store = new EFEventStore(GetContext()))
                {
                    var collection = await store.GetEventsFromAggregateIdAsync<SampleAgg>(agg.AggregateUniqueId).ConfigureAwait(false);
                    collection.Should().HaveCount(2);

                    collection.Any(e => e.GetType() == typeof(AggCreated)).Should().BeTrue();
                    collection.Any(e => e.GetType() == typeof(AggDeleted)).Should().BeTrue();
                    collection.All(e => e.AggregateId == agg.AggregateUniqueId).Should().BeTrue();
                    collection.First().Should().BeOfType<AggCreated>();
                    collection.Skip(1).First().Should().BeOfType<AggDeleted>();
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        #endregion

        #region Snapshot behavior

        private class AggregateSnapshotEvent : BaseDomainEvent
        {
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
        public async Task EFEventStore_StoreDomainEventAsync_CreateSnapshot()
        {
            EventStoreManager.Behaviors = new Dictionary<Type, ISnapshotBehavior>
            {
                {typeof(AggregateSnapshotEvent), new NumericSnapshotBehavior(10) }
            };
            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                for (int i = 0; i < 11; i++)
                {
                    using (var store = new EFEventStore(GetContext()))
                    {
                        await store.StoreDomainEventAsync(new AggregateSnapshotEvent(aggId)).ConfigureAwait(false);
                    }
                }

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count().Should().Be(1);
                    var evt = ctx.Set<Event>().FirstOrDefault();
                    evt.Should().NotBeNull();
                    evt.AggregateId.Should().Be(aggId);
                    evt.Sequence.Should().Be(1);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();

                    ctx.Set<Snapshot>().Count().Should().Be(1);
                    var snap = ctx.Set<Snapshot>().FirstOrDefault();
                    snap.Should().NotBeNull();
                    snap.AggregateId.Should().Be(aggId);
                    snap.AggregateType.Should().Be(typeof(AggregateSnapshot).AssemblyQualifiedName);

                    using (var store = new EFEventStore(GetContext()))
                    {
                        var agg = await store.GetRehydratedAggregateAsync<AggregateSnapshot>(aggId).ConfigureAwait(false);
                        agg.Should().NotBeNull();
                        agg.AggIncValue.Should().Be(11);
                    }
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFEventStore_StoreDomainEventAsync_CreateSnapshot_Multiple_Same_Aggregates()
        {
            EventStoreManager.Behaviors = new Dictionary<Type, ISnapshotBehavior>
            {
                {typeof(AggregateSnapshotEvent), new NumericSnapshotBehavior(10) }
            };
            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                using (var store = new EFEventStore(GetContext()))
                {
                    for (int i = 0; i < 11; i++)
                    {
                        await store.StoreDomainEventAsync(new AggregateSnapshotEvent(aggId)).ConfigureAwait(false);
                    }
                    var otherId = Guid.NewGuid();
                    for (int i = 0; i < 30; i++)
                    {
                        if (i % 10 == 0)
                        {
                            otherId = Guid.NewGuid();
                        }
                        await store.StoreDomainEventAsync(new AggregateSnapshotEvent(otherId)).ConfigureAwait(false);
                    }
                }
                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count(e => e.AggregateId == aggId).Should().Be(1);
                    var evt = ctx.Set<Event>().Where(e => e.AggregateId == aggId).FirstOrDefault();
                    evt.Should().NotBeNull();
                    evt.AggregateId.Should().Be(aggId);
                    evt.Sequence.Should().Be(1);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();

                    ctx.Set<Snapshot>().Count().Should().Be(1);
                    var snap = ctx.Set<Snapshot>().FirstOrDefault();
                    snap.Should().NotBeNull();
                    snap.AggregateId.Should().Be(aggId);
                    snap.AggregateType.Should().Be(typeof(AggregateSnapshot).AssemblyQualifiedName);

                    using (var store = new EFEventStore(GetContext()))
                    {
                        var agg = await store.GetRehydratedAggregateAsync<AggregateSnapshot>(aggId).ConfigureAwait(false);
                        agg.Should().NotBeNull();
                        agg.AggIncValue.Should().Be(11);
                    }
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFEventStore_StoreDomainEventAsync_NoSnapshotBehaviorDefined()
        {
            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                for (int i = 0; i < 11; i++)
                {
                    using (var store = new EFEventStore(GetContext()))
                    {
                        await store.StoreDomainEventAsync(new AggregateSnapshotEvent(aggId)).ConfigureAwait(false);
                    }
                }

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count().Should().Be(11);

                    ctx.Set<Snapshot>().Count().Should().Be(0);

                    using (var store = new EFEventStore(GetContext()))
                    {
                        var agg = await store.GetRehydratedAggregateAsync<AggregateSnapshot>(aggId).ConfigureAwait(false);
                        agg.Should().NotBeNull();
                        agg.AggIncValue.Should().Be(11);
                    }
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
