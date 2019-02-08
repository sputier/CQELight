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
using Moq;
using Microsoft.EntityFrameworkCore;
using System.IO;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CQELight.EventStore.EFCore.Integration.Tests
{
    public class EFEventStoreTests : BaseUnitTestClass
    {
        #region Ctor & members

        private static bool s_Init;

        private Mock<ISnapshotBehaviorProvider> _snapshotProviderMock;
        public EFEventStoreTests()
        {
            _snapshotProviderMock = new Mock<ISnapshotBehaviorProvider>();
            if (!s_Init)
            {
                using (var ctx = GetContext())
                {
                    ctx.Database.EnsureDeleted();
                    ctx.Database.EnsureCreated();
                }
                using (var aCtx = GetArchiveContext())
                {
                    aCtx.Database.EnsureDeleted();
                    aCtx.Database.EnsureCreated();
                }
                s_Init = true;
            }
            DeleteAll();
            EventStoreManager.ArchiveBehaviorInfos = null;
            EventStoreManager.BufferInfo = null;
            EventStoreManager.DbContextOptions = null;
            EventStoreManager.SnapshotBehaviorProvider = null;
            EventStoreManager.Deactivate();
        }

        private DbContextOptions<EventStoreDbContext> GetBaseOptions()
            => new DbContextOptionsBuilder<EventStoreDbContext>()
                    //.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Events_Tests_Base;Trusted_Connection=True;MultipleActiveResultSets=true;")
                    .UseSqlite("Filename=Events_Test_Base.db")
                    .Options;
        private DbContextOptions<ArchiveEventStoreDbContext> GetArchiveOptions()
            => new DbContextOptionsBuilder<ArchiveEventStoreDbContext>()
                    //.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EventsArchive_Tests_Base;Trusted_Connection=True;MultipleActiveResultSets=true;")
                    .UseSqlite("Filename=Events_Test_Archive_Base.db")
                    .Options;

        private EventStoreDbContext GetContext()
            => new EventStoreDbContext(GetBaseOptions());

        private ArchiveEventStoreDbContext GetArchiveContext()
            => new ArchiveEventStoreDbContext(GetArchiveOptions());

        private void DeleteAll()
        {
            using (var ctx = GetContext())
            {
                ctx.RemoveRange(ctx.Set<Event>());
                ctx.RemoveRange(ctx.Set<Snapshot>());
                ctx.SaveChanges();
            }
            using (var ctx = GetArchiveContext())
            {
                ctx.RemoveRange(ctx.Set<ArchiveEvent>());
                ctx.SaveChanges();
            }
        }

        private Task StoreTestEventAsync(Guid aggId, Guid id, DateTime date)
            => new EFEventStore(GetBaseOptions()).StoreDomainEventAsync(new SampleEvent(aggId, id, date)
            {
                Data = "testData"
            });

        #region Nested class

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

        public class SampleAgg : EventSourcedAggregate<Guid>
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

        [EventNotPersisted]
        private class NotPersistedEvent : BaseDomainEvent
        {

        }

        [Fact]
        public async Task StoreDomainEventAsync_NotPersistedEvent_Should_NotBePersisted()
        {
            try
            {
                DeleteAll();
                await new EFEventStore(GetBaseOptions()).StoreDomainEventAsync(new NotPersistedEvent());

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
        public async Task StoreDomainEventAsync_SimpleEvent_Should_Be_StoredInDatabase()
        {
            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                Guid id = Guid.NewGuid();
                await StoreTestEventAsync(aggId, id, DateTime.Today);

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count(e => e.HashedAggregateId == aggId.ToJson().GetHashCode()).Should().Be(1);
                    var evt = ctx.Set<Event>().FirstOrDefault();
                    evt.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    evt.Id.Should().Be(id);
                    evt.EventTime.Should().BeSameDateAs(DateTime.Today);
                    evt.Sequence.Should().Be(1);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task StoreDomainEventAsync_Multiples_WithoutBuffer_Should_BeStoredInDatabase()
        {
            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid(); ;
                var store = new EFEventStore(GetBaseOptions());
                for (int i = 0; i < 100; i++)
                {
                    await store.StoreDomainEventAsync(new SampleEvent(aggId, Guid.NewGuid(), DateTime.Today)
                    {
                        Data = "testData" + i
                    });
                }

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count(e => e.HashedAggregateId == aggId.ToJson().GetHashCode()).Should().Be(100);
                    var evt = ctx.Set<Event>().FirstOrDefault();
                    evt.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    evt.EventTime.Should().BeSameDateAs(DateTime.Today);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task StoreDomainEventAsync_Multiples_WithBuffer_Should_BeStoredInDatabase()
        {
            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                var store = new EFEventStore(
                    GetBaseOptions(),
                    bufferInfo: new BufferInfo(new TimeSpan(0, 0, 2), new TimeSpan(0, 0, 2)));
                for (int i = 0; i < 100; i++)
                {
                    await store.StoreDomainEventAsync(new SampleEvent(aggId, Guid.NewGuid(), DateTime.Today)
                    {
                        Data = "testData" + i
                    });
                }

                await Task.Delay(2500);

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().AsNoTracking().Count(e => e.HashedAggregateId == aggId.ToJson().GetHashCode()).Should().Be(100);
                    var evt = ctx.Set<Event>().AsNoTracking().FirstOrDefault();
                    evt.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    evt.EventTime.Should().BeSameDateAs(DateTime.Today);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();
                    await ctx.Set<Event>().AsNoTracking().AllAsync(e => e.EventData.Contains("testData" + (e.Sequence - 1)));
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
        public async Task GetEventByIdAsync_IdNotFound_Should_ReturnsNull()
        {
            try
            {
                DeleteAll();
                (await new EFEventStore(GetBaseOptions()).GetEventByIdAsync<SampleEvent>(Guid.NewGuid())).Should().BeNull();
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task GetEventByIdAsync_WithId_Should_ReturnsEvent()
        {
            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                Guid id = Guid.Empty;
                var store = new EFEventStore(GetBaseOptions());

                for (int i = 0; i < 100; i++)
                {
                    var eventId = Guid.NewGuid();
                    await store.StoreDomainEventAsync(new SampleEvent(aggId, eventId, DateTime.Today)
                    {
                        Data = "testData" + i
                    });
                    if (i == 50)
                    {
                        id = eventId;
                    }
                }

                var store2 = new EFEventStore(GetBaseOptions());
                var evt = await store2.GetEventByIdAsync<SampleEvent>(id);
                evt.Should().NotBeNull();
                evt.AggregateId.Should().Be(aggId);
                evt.Id.Should().Be(id);
                evt.EventTime.Should().BeSameDateAs(DateTime.Today);
                evt.Sequence.Should().Be(51);
                evt.Data.Should().Be("testData50");
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
                new Bootstrapper()
                    .UseEFCoreAsEventStore(
                    new EFCoreEventStoreBootstrapperConfigurationOptions(c =>
                        //c.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Events_Tests_Base;Trusted_Connection=True;MultipleActiveResultSets=true;")
                        c.UseSqlite("Filename=Events_Test_Base.db")))
                    .Bootstrapp();
                var agg = new SampleAgg();
                agg.SimulateWork();
                await agg.PublishDomainEventsAsync();

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Should().HaveCount(2);
                }

                var collection = await (await new EFEventStore(GetBaseOptions()).GetEventsFromAggregateIdAsync<SampleAgg, Guid>(agg.Id)).ToList();
                collection.Should().HaveCount(2);

                collection.Any(e => e.GetType() == typeof(AggCreated)).Should().BeTrue();
                collection.Any(e => e.GetType() == typeof(AggDeleted)).Should().BeTrue();
                collection.All(e => e.AggregateId is Guid guidId && guidId == agg.Id).Should().BeTrue();
                collection[0].Should().BeOfType<AggCreated>();
                collection.Skip(1).First().Should().BeOfType<AggDeleted>();
            }
            finally
            {
                EventStoreManager.Deactivate();
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
                EventTime = DateTime.Now;
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
        public async Task SnapshoptBehavior_Shouldnt_Be_Used_If_Set_To_Disabled()
        {
            _snapshotProviderMock.Setup(m => m.GetBehaviorForEventType(typeof(AggregateSnapshotEvent))).Returns(new NumericSnapshotBehavior(10));

            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                var store = new EFEventStore(GetBaseOptions(), archiveBehaviorInfos: new EventArchiveBehaviorInfos
                {
                    ArchiveBehavior = SnapshotEventsArchiveBehavior.Disabled
                });
                for (int i = 0; i < 11; i++)
                {
                    var evt = new AggregateSnapshotEvent(aggId);
                    await store.StoreDomainEventAsync(evt);
                }


                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count().Should().Be(11);
                    var evt = await ctx.Set<Event>().OrderByDescending(e => e.EventTime).FirstOrDefaultAsync();
                    evt.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    evt.Sequence.Should().Be(11);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();

                    ctx.Set<Snapshot>().Count().Should().Be(0);
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task SnapshotBehavior_Should_RemoveEvent_If_DeleteOptions_IsChoosen()
        {
            int i = 0;
            List<IDomainEvent> events = new List<IDomainEvent>();
            var behaviorMock = new Mock<ISnapshotBehavior>();
            behaviorMock.Setup(m => m.IsSnapshotNeededAsync(It.IsAny<object>(), It.IsAny<Type>()))
                .ReturnsAsync((object o, Type t) => i == 10);
            behaviorMock.Setup(m => m.GenerateSnapshotAsync(It.IsAny<object>(), It.IsAny<Type>(), It.IsAny<IEventSourcedAggregate>()))
                .ReturnsAsync((object o, Type t, IEventSourcedAggregate agg) => (new Snapshot
                {
                    AggregateId = o,
                    AggregateType = t.AssemblyQualifiedName,
                    HashedAggregateId = 0,
                    SnapshotData = "{}",
                    SnapshotBehaviorType = "test",
                    SnapshotTime = DateTime.Now
                }, events));
            _snapshotProviderMock.Setup(m => m.GetBehaviorForEventType(typeof(AggregateSnapshotEvent))).Returns(behaviorMock.Object);

            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                var store = new EFEventStore(
                                                GetBaseOptions(),
                                                snapshotBehaviorProvider: _snapshotProviderMock.Object,
                                                archiveBehaviorInfos: new EventArchiveBehaviorInfos
                                                {
                                                    ArchiveBehavior = SnapshotEventsArchiveBehavior.Delete
                                                });
                for (i = 0; i < 11; i++)
                {
                    var evt = new AggregateSnapshotEvent(aggId);
                    await store.StoreDomainEventAsync(evt);
                    if (i < 10)
                    {
                        events.Add(evt);
                    }
                }

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count().Should().Be(1);
                    var evt = ctx.Set<Event>().FirstOrDefault();
                    evt.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    evt.Sequence.Should().Be(11);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFEventStore_StoreDomainEventAsync_CreateSnapshot()
        {
            _snapshotProviderMock.Setup(m => m.GetBehaviorForEventType(typeof(AggregateSnapshotEvent)))
                .Returns(new NumericSnapshotBehavior(10, GetBaseOptions()));

            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                var store = new EFEventStore(GetBaseOptions(),
                    snapshotBehaviorProvider: _snapshotProviderMock.Object,
                    archiveBehaviorInfos: new EventArchiveBehaviorInfos
                    {
                        ArchiveBehavior = SnapshotEventsArchiveBehavior.Delete
                    });
                for (int i = 0; i < 11; i++)
                {
                    await store.StoreDomainEventAsync(new AggregateSnapshotEvent(aggId));
                }

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count().Should().Be(1);
                    var evt = ctx.Set<Event>().FirstOrDefault();
                    evt.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    evt.Sequence.Should().Be(11);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();

                    ctx.Set<Snapshot>().Count().Should().Be(1);
                    var snap = ctx.Set<Snapshot>().FirstOrDefault();
                    snap.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    snap.AggregateType.Should().Be(typeof(AggregateSnapshot).AssemblyQualifiedName);

                    var agg = await new EFEventStore(GetBaseOptions()).GetRehydratedAggregateAsync<AggregateSnapshot, Guid>(aggId);
                    agg.Should().NotBeNull();
                    agg.AggIncValue.Should().Be(11);
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFEventStore_StoreDomainEventAsync_CreateSnapshot_Second_Should_Erase_FirstSnapshot()
        {
            _snapshotProviderMock.Setup(m => m.GetBehaviorForEventType(typeof(AggregateSnapshotEvent))).Returns(new NumericSnapshotBehavior(10, GetBaseOptions()));
            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                var store = new EFEventStore(GetBaseOptions(),
                    snapshotBehaviorProvider: _snapshotProviderMock.Object,
                    archiveBehaviorInfos: new EventArchiveBehaviorInfos
                    {
                        ArchiveBehavior = SnapshotEventsArchiveBehavior.StoreToNewTable
                    });
                for (int i = 0; i < 21; i++)
                {
                    await store.StoreDomainEventAsync(new AggregateSnapshotEvent(aggId));
                }

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count().Should().Be(1);
                    var evt = ctx.Set<Event>().FirstOrDefault();
                    evt.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    evt.Sequence.Should().Be(21);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();

                    ctx.Set<Snapshot>().Count().Should().Be(1);
                    var snap = ctx.Set<Snapshot>().FirstOrDefault();
                    snap.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    snap.AggregateType.Should().Be(typeof(AggregateSnapshot).AssemblyQualifiedName);

                    var agg = await new EFEventStore(GetBaseOptions()).GetRehydratedAggregateAsync<AggregateSnapshot, Guid>(aggId);
                    agg.Should().NotBeNull();
                    agg.AggIncValue.Should().Be(21);
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFEventStore_StoreDomainEventAsync_CreateSnapshot_Archive_In_DifferentTable()
        {
            _snapshotProviderMock.Setup(m => m.GetBehaviorForEventType(typeof(AggregateSnapshotEvent))).Returns(new NumericSnapshotBehavior(10, GetBaseOptions()));
            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                var store = new EFEventStore(GetBaseOptions(),
                    snapshotBehaviorProvider: _snapshotProviderMock.Object,
                    archiveBehaviorInfos: new EventArchiveBehaviorInfos
                    {
                        ArchiveBehavior = SnapshotEventsArchiveBehavior.StoreToNewTable
                    });

                for (int i = 0; i < 11; i++)
                {
                    await store.StoreDomainEventAsync(new AggregateSnapshotEvent(aggId));
                }

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Where(c => c.HashedAggregateId == aggId.ToJson(true).GetHashCode()).Count().Should().Be(1);
                    ctx.Set<ArchiveEvent>().Where(c => c.HashedAggregateId == aggId.ToJson(true).GetHashCode()).Count().Should().Be(10);
                    var evt = ctx.Set<Event>().FirstOrDefault();
                    evt.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    evt.Sequence.Should().Be(11);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();

                    ctx.Set<Snapshot>().Count().Should().Be(1);
                    var snap = ctx.Set<Snapshot>().FirstOrDefault();
                    snap.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    snap.AggregateType.Should().Be(typeof(AggregateSnapshot).AssemblyQualifiedName);

                    var agg = await new EFEventStore(GetBaseOptions()).GetRehydratedAggregateAsync<AggregateSnapshot, Guid>(aggId);
                    agg.Should().NotBeNull();
                    agg.AggIncValue.Should().Be(11);
                }
            }
            finally
            {
                DeleteAll();
            }
        }

        [Fact]
        public async Task EFEventStore_StoreDomainEventAsync_CreateSnapshot_Archive_In_DifferentDatabase()
        {
            _snapshotProviderMock.Setup(m => m.GetBehaviorForEventType(typeof(AggregateSnapshotEvent))).Returns(new NumericSnapshotBehavior(10, GetBaseOptions()));
            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                var store = new EFEventStore(GetBaseOptions(),
                    snapshotBehaviorProvider: _snapshotProviderMock.Object,
                    archiveBehaviorInfos: new EventArchiveBehaviorInfos
                    {
                        ArchiveBehavior = SnapshotEventsArchiveBehavior.StoreToNewDatabase,
                        ArchiveDbContextOptions = GetArchiveOptions()
                    });
                for (int i = 0; i < 11; i++)
                {
                    await store.StoreDomainEventAsync(new AggregateSnapshotEvent(aggId));
                }

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count().Should().Be(1);
                    var evt = ctx.Set<Event>().FirstOrDefault();
                    evt.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    evt.Sequence.Should().Be(11);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();

                    ctx.Set<Snapshot>().Count().Should().Be(1);
                    var snap = ctx.Set<Snapshot>().FirstOrDefault();
                    snap.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    snap.AggregateType.Should().Be(typeof(AggregateSnapshot).AssemblyQualifiedName);

                    var agg = await new EFEventStore(GetBaseOptions()).GetRehydratedAggregateAsync<AggregateSnapshot, Guid>(aggId);
                    agg.Should().NotBeNull();
                    agg.AggIncValue.Should().Be(11);
                }

                using (var aCtx = GetArchiveContext())
                {
                    aCtx.Set<ArchiveEvent>().Count().Should().Be(10);
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
            _snapshotProviderMock.Setup(m => m.GetBehaviorForEventType(typeof(AggregateSnapshotEvent))).Returns(new NumericSnapshotBehavior(10, GetBaseOptions()));
            try
            {
                DeleteAll();
                Guid aggId = Guid.NewGuid();
                var store = new EFEventStore(GetBaseOptions(),
                    snapshotBehaviorProvider: _snapshotProviderMock.Object,
                    archiveBehaviorInfos: new EventArchiveBehaviorInfos
                    {
                        ArchiveBehavior = SnapshotEventsArchiveBehavior.Delete
                    });
                for (int i = 0; i < 11; i++)
                {
                    await store.StoreDomainEventAsync(new AggregateSnapshotEvent(aggId));
                }
                var otherId = Guid.NewGuid();
                for (int i = 0; i < 30; i++)
                {
                    if (i % 10 == 0)
                    {
                        otherId = Guid.NewGuid();
                    }
                    await store.StoreDomainEventAsync(new AggregateSnapshotEvent(otherId));
                }

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count(e => e.HashedAggregateId == aggId.ToJson(true).GetHashCode()).Should().Be(1);
                    var evt = ctx.Set<Event>().Where(e => e.HashedAggregateId == aggId.ToJson(true).GetHashCode()).FirstOrDefault();
                    evt.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    evt.Sequence.Should().Be(11);
                    evt.EventData.Should().NotBeNullOrWhiteSpace();

                    ctx.Set<Snapshot>().Count().Should().Be(1);
                    var snap = ctx.Set<Snapshot>().FirstOrDefault();
                    snap.Should().NotBeNull();
                    evt.HashedAggregateId.Should().Be(aggId.ToJson(true).GetHashCode());
                    snap.AggregateType.Should().Be(typeof(AggregateSnapshot).AssemblyQualifiedName);

                    var agg = await new EFEventStore(GetBaseOptions()).GetRehydratedAggregateAsync<AggregateSnapshot, Guid>(aggId);
                    agg.Should().NotBeNull();
                    agg.AggIncValue.Should().Be(11);
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
                var store = new EFEventStore(GetBaseOptions());
                for (int i = 0; i < 11; i++)
                {
                    await store.StoreDomainEventAsync(new AggregateSnapshotEvent(aggId));
                }

                using (var ctx = GetContext())
                {
                    ctx.Set<Event>().Count().Should().Be(11);

                    ctx.Set<Snapshot>().Count().Should().Be(0);

                    var agg = await new EFEventStore(GetBaseOptions()).GetRehydratedAggregateAsync<AggregateSnapshot, Guid>(aggId);
                    agg.Should().NotBeNull();
                    agg.AggIncValue.Should().Be(11);
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
