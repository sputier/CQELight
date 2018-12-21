using BenchmarkDotNet.Attributes;
using CQELight;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore;
using CQELight.EventStore.EFCore;
using CQELight.EventStore.EFCore.Common;
using CQELight.EventStore.EFCore.Models;
using CQELight.EventStore.EFCore.Snapshots;
using CQELight_Benchmarks.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks.Benchmarks
{
    public enum DatabaseType
    {
        SQLite,
        SQLServer
    }
    public class EFCore_EventStoreBenchmark
    {
        #region BenchmarkDotNet

        public Guid AggregateId = Guid.NewGuid();

        [Params(DatabaseType.SQLite, DatabaseType.SQLServer)]
        public DatabaseType DatabaseType;

        [GlobalSetup]
        public void GlobalSetup()
        {
            CreateDatabase(DatabaseType);
            CleanDatabases();

        }

        [IterationSetup(Targets = new[] { nameof(StoreRangeDomainEvent), nameof(StoreRangeDomainEvent_Snapshot) })]
        public void IterationSetup()
        {
            CleanDatabases();
        }

        [GlobalSetup(Targets = new[] { nameof(RehydrateAggregate) })]
        public void GlobalSetup_Storage()
        {
            CreateDatabase(DatabaseType);
            CleanDatabases();
            StoreNDomainEvents();
        }

        [GlobalSetup(Targets = new[] { nameof(RehydrateAggregate_WithSnapshot) })]
        public void GlobalSetup_Storage_Snapshot()
        {
            CreateDatabase(DatabaseType);
            CleanDatabases();
            StoreNDomainEvents(new BasicSnapshotBehaviorProvider(new Dictionary<Type, ISnapshotBehavior>()
            {
                {typeof(TestEvent), new NumericSnapshotBehavior( 10) }
            }));
        }

        internal static void CreateDatabase(DatabaseType databaseType)
        {
            EventStoreManager.DbContextOptions =
                databaseType == DatabaseType.SQLite
                ? new DbContextOptionsBuilder().UseSqlite(GetConnectionString_SQLite()).Options
                : new DbContextOptionsBuilder().UseSqlServer(GetConnectionString_SQLServer()).Options;
            using (var ctx = new EventStoreDbContext(EventStoreManager.DbContextOptions))
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();
            }
        }

        #endregion

        #region Private methods

        private void CleanDatabases()
        {
            using (var ctx = new EventStoreDbContext(GetConfig()))
            {
                ctx.RemoveRange(ctx.Set<Event>());
                ctx.RemoveRange(ctx.Set<Snapshot>());
                ctx.SaveChanges();
            }
        }

        private void StoreNDomainEvents(ISnapshotBehaviorProvider provider = null)
        {
            var store = new EFEventStore(GetConfig(), snapshotBehaviorProvider: provider);
            for (int i = 0; i < 1000; i++)
            {
                store.StoreDomainEventAsync(new TestEvent(Guid.NewGuid(), AggregateId) { AggregateStringValue = "test", AggregateIntValue = i }).GetAwaiter().GetResult();
            }
        }

        private static string GetConnectionString_SQLServer()
            => new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["EFCore_EventStore_Benchmarks:ConnectionString_SQLServer"];

        private static string GetConnectionString_SQLite()
            => new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["EFCore_EventStore_Benchmarks:ConnectionString_SQLite"];

        private DbContextOptions GetConfig()
        {
            switch (DatabaseType)
            {
                case DatabaseType.SQLite: return new DbContextOptionsBuilder().UseSqlite(GetConnectionString_SQLite()).Options;
                default: return new DbContextOptionsBuilder().UseSqlServer(GetConnectionString_SQLServer()).Options;
            }
        }

        #endregion

        #region Public methods

        [Benchmark]
        public async Task StoreSingleDomainEvent()
        {
            await new EFEventStore(GetConfig()).StoreDomainEventAsync(
            new TestEvent(Guid.NewGuid(), AggregateId)
            {
                AggregateIntValue = 1,
                AggregateStringValue = "test"
            });
        }

        [Benchmark]
        [Arguments(10, true)]
        [Arguments(10, false)]
        [Arguments(100, true)]
        [Arguments(100, false)]
        [Arguments(1000, true)]
        [Arguments(1000, false)]
        public async Task StoreRangeDomainEvent(int numberEvents, bool useBuffer)
        {
            if (useBuffer)
            {
                EventStoreManager.BufferInfo = BufferInfo.Default;
            }
            else
            {
                EventStoreManager.BufferInfo = BufferInfo.Disabled;
            }
            var store = new EFEventStore(GetConfig());
            for (int i = 0; i < numberEvents; i++)
            {
                await store.StoreDomainEventAsync(
                    new TestEvent(Guid.NewGuid(), AggregateId)
                    {
                        AggregateIntValue = 1,
                        AggregateStringValue = "test"
                    });
            }
        }


        [Benchmark]
        [Arguments(10, true)]
        [Arguments(10, false)]
        [Arguments(100, true)]
        [Arguments(100, false)]
        [Arguments(1000, true)]
        [Arguments(1000, false)]
        public async Task StoreRangeDomainEvent_Snapshot(int numberEvents, bool useBuffer)
        {
            if (useBuffer)
            {
                EventStoreManager.BufferInfo = BufferInfo.Default;
            }
            else
            {
                EventStoreManager.BufferInfo = BufferInfo.Disabled;
            }
            var store = new EFEventStore(GetConfig(),
                snapshotBehaviorProvider: new BasicSnapshotBehaviorProvider(new Dictionary<Type, ISnapshotBehavior>()
                    { {typeof(TestEvent), new NumericSnapshotBehavior(10,GetConfig()) }}));
            for (int i = 0; i < numberEvents; i++)
            {
                await store.StoreDomainEventAsync(
                   new TestEvent(Guid.NewGuid(), AggregateId)
                   {
                       AggregateIntValue = 1,
                       AggregateStringValue = "test"
                   });
            }
        }

        [Benchmark]
        public async Task GetEventsByAggregateId()
        {
            var evt
            = await new EFEventStore(GetConfig()).GetEventsFromAggregateIdAsync<BenchmarkSimpleEvent>
            (
               AggregateId
            );
        }

        [Benchmark]
        public async Task RehydrateAggregate()
        {
            var store = new EFEventStore(GetConfig());
            var agg = await store.GetRehydratedAggregateAsync<TestAggregate>(AggregateId);
        }

        [Benchmark]
        public async Task RehydrateAggregate_WithSnapshot()
        {
            var store = new EFEventStore(GetConfig(), snapshotBehaviorProvider: new BasicSnapshotBehaviorProvider(new Dictionary<Type, ISnapshotBehavior>()
                    { {typeof(TestEvent), new NumericSnapshotBehavior(10, GetConfig()) }}));
            var agg = await store.GetRehydratedAggregateAsync<TestAggregate>(AggregateId);

        }

        #endregion
    }
}
