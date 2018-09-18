using BenchmarkDotNet.Attributes;
using CQELight;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore;
using CQELight.EventStore.EFCore;
using CQELight.EventStore.EFCore.Common;
using CQELight.EventStore.EFCore.Models;
using CQELight.EventStore.EFCore.Snapshots;
using CQELight_Benchmarks.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks.Benchmarks
{
    public class EFCore_EventStoreBenchmark : BaseBenchmark
    {
        #region BenchmarkDotNet

        [Params(ConfigurationType.SQLite, ConfigurationType.SQLServer)]
        public ConfigurationType ConfigurationType;

        [GlobalSetup]
        public void GlobalSetup()
        {
            CreateDatabase(ConfigurationType);
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
            CreateDatabase(ConfigurationType);
            CleanDatabases();
            StoreNDomainEvents();
        }

        [GlobalSetup(Targets = new[] { nameof(RehydrateAggregate_WithSnapshot) })]
        public void GlobalSetup_Storage_Snapshot()
        {
            CreateDatabase(ConfigurationType);
            CleanDatabases();
            StoreNDomainEvents(new BasicSnapshotBehaviorProvider(new Dictionary<Type, ISnapshotBehavior>()
            {
                {typeof(TestEvent), new NumericSnapshotBehavior( 10) }
            }));
        }

        internal static void CreateDatabase(ConfigurationType configuration)
        {
            EventStoreManager.DbContextConfiguration = new DbContextConfiguration
            {
                ConfigType = configuration,
                ConnectionString = configuration == ConfigurationType.SQLServer ? GetConnectionString_SQLServer() : GetConnectionString_SQLite()
            };
            using (var ctx = new EventStoreDbContext(EventStoreManager.DbContextConfiguration))
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();
            }
        }

        #endregion

        #region Private methods

        private void CleanDatabases()
        {
            using (var ctx = new EventStoreDbContext(new DbContextConfiguration
            {
                ConfigType = ConfigurationType,
                ConnectionString = ConfigurationType == ConfigurationType.SQLServer ? GetConnectionString_SQLServer() : GetConnectionString_SQLite()
            }))
            {
                ctx.RemoveRange(ctx.Set<Event>());
                ctx.RemoveRange(ctx.Set<Snapshot>());
                ctx.SaveChanges();
            }
        }

        private void StoreNDomainEvents(ISnapshotBehaviorProvider provider = null)
        {
            using (var ctx = new EventStoreDbContext(new DbContextConfiguration
            {
                ConfigType = ConfigurationType,
                ConnectionString = ConfigurationType == ConfigurationType.SQLServer ? GetConnectionString_SQLServer() : GetConnectionString_SQLite()
            }))
            {
                var store = new EFEventStore(ctx, snapshotBehaviorProvider: provider);
                for (int i = 0; i < N; i++)
                {
                    store.StoreDomainEventAsync(new TestEvent(Guid.NewGuid(), AggregateId) { AggregateStringValue = "test", AggregateIntValue = N }).GetAwaiter().GetResult();
                }
            }
        }

        private static string GetConnectionString_SQLServer()
            => new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["EFCore_EventStore_Benchmarks:ConnectionString_SQLServer"];

        private static string GetConnectionString_SQLite()
            => new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["EFCore_EventStore_Benchmarks:ConnectionString_SQLite"];

        private DbContextConfiguration GetConfig()
            => new DbContextConfiguration
            {
                ConfigType = ConfigurationType,
                ConnectionString = ConfigurationType == ConfigurationType.SQLServer ? GetConnectionString_SQLServer() : GetConnectionString_SQLite()
            };

        #endregion

        #region Public methods

        [Benchmark]
        public async Task StoreDomainEvent()
        {
            using (var ctx = new EventStoreDbContext(GetConfig()))
            {
                await new EFEventStore(ctx).StoreDomainEventAsync(
                new TestEvent(Guid.NewGuid(), AggregateId)
                {
                    AggregateIntValue = 1,
                    AggregateStringValue = "test"
                });
            }
        }

        [Benchmark]
        public async Task StoreRangeDomainEvent()
        {
            using (var ctx = new EventStoreDbContext(GetConfig()))
            {
                var store = new EFEventStore(ctx);
                for (int i = 0; i < N; i++)
                {
                    await store.StoreDomainEventAsync(
                        new TestEvent(Guid.NewGuid(), AggregateId)
                        {
                            AggregateIntValue = 1,
                            AggregateStringValue = "test"
                        });
                }
            }
        }


        [Benchmark]
        public async Task StoreRangeDomainEvent_Snapshot()
        {
            using (var ctx = new EventStoreDbContext(GetConfig()))
            {
                var store = new EFEventStore(ctx, snapshotBehaviorProvider: new BasicSnapshotBehaviorProvider(new Dictionary<Type, ISnapshotBehavior>()
                    { {typeof(TestEvent), new NumericSnapshotBehavior(10,GetConfig()) }}));
                for (int i = 0; i < N; i++)
                {
                    await store.StoreDomainEventAsync(
                       new TestEvent(Guid.NewGuid(), AggregateId)
                       {
                           AggregateIntValue = 1,
                           AggregateStringValue = "test"
                       });
                }
            }
        }

        [Benchmark]
        public async Task GetEventsByAggregateId()
        {
            using (var ctx = new EventStoreDbContext(GetConfig()))
            {
                var evt
                = await new EFEventStore(ctx).GetEventsFromAggregateIdAsync<BenchmarkSimpleEvent>
                (
                   AggregateId
                );
            }
        }

        [Benchmark]
        public async Task RehydrateAggregate()
        {
            using (var ctx = new EventStoreDbContext(GetConfig()))
            {
                var store = new EFEventStore(ctx);
                var agg = await store.GetRehydratedAggregateAsync<TestAggregate>(AggregateId);
            }

        }

        [Benchmark]
        public async Task RehydrateAggregate_WithSnapshot()
        {
            using (var ctx = new EventStoreDbContext(GetConfig()))
            {
                var store = new EFEventStore(ctx, snapshotBehaviorProvider: new BasicSnapshotBehaviorProvider(new Dictionary<Type, ISnapshotBehavior>()
                    { {typeof(TestEvent), new NumericSnapshotBehavior(10, GetConfig()) }}));
                var agg = await store.GetRehydratedAggregateAsync<TestAggregate>(AggregateId);
            }

        }

        #endregion
    }
}
