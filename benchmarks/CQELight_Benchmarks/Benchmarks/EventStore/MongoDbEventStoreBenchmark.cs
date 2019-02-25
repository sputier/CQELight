using BenchmarkDotNet.Attributes;
using CQELight;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Dispatcher;
using CQELight.EventStore;
using CQELight.EventStore.MongoDb;
using CQELight.EventStore.MongoDb.Snapshots;
using CQELight_Benchmarks.Custom;
using CQELight_Benchmarks.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks.Benchmarks
{
    public class MongoDbEventStoreBenchmark : BaseBenchmark
    {

        #region BenchmarkDotNet

        [GlobalSetup]
        public void GlobalSetup()
        {
            AggregateId = Guid.NewGuid();
            new Bootstrapper().UseMongoDbAsEventStore(new MongoEventStoreOptions(GetMongoDbUrl())).Bootstrapp();
        }

        [IterationSetup(Targets = new[] { nameof(StoreRangeDomainEvent), nameof(StoreRangeDomainEvent_Snapshot) })]
        public void IterationSetup()
        {
            CleanDatases();
        }

        [GlobalSetup(Targets = new[] { nameof(RehydrateAggregate) })]
        public void GlobalSetup_Storage()
        {
            new Bootstrapper().UseMongoDbAsEventStore(new MongoEventStoreOptions(GetMongoDbUrl())).Bootstrapp();
            CleanDatases();
            StoreNDomainEvents();
        }

        [GlobalSetup(Targets = new[] { nameof(RehydrateAggregate_WithSnapshot) })]
        public void GlobalSetup_Storage_Snapshot()
        {
            new Bootstrapper().UseMongoDbAsEventStore(new MongoEventStoreOptions(GetMongoDbUrl())).Bootstrapp();
            CleanDatases();
            StoreNDomainEvents(new BasicSnapshotBehaviorProvider(new Dictionary<Type, ISnapshotBehavior>()
            {
                {typeof(TestEvent), new NumericSnapshotBehavior( 10) }
            }));
        }

        #endregion

        #region Private methods

        private void CleanDatases()
        {
            var client = new MongoClient(GetMongoDbUrl());
            var db = client.GetDatabase(CQELight.EventStore.MongoDb.Consts.CONST_DB_NAME);
            db.DropCollection(CQELight.EventStore.MongoDb.Consts.CONST_EVENTS_COLLECTION_NAME);
            db.DropCollection(CQELight.EventStore.MongoDb.Consts.CONST_SNAPSHOT_COLLECTION_NAME);
        }

        private void StoreNDomainEvents(ISnapshotBehaviorProvider provider = null)
        {
            EventStoreManager.Client = new MongoClient(GetMongoDbUrl());
            var store = new MongoDbEventStore(provider);
            for (int i = 0; i < N; i++)
            {
                store.StoreDomainEventAsync(new TestEvent(Guid.NewGuid(), AggregateId) { AggregateStringValue = "test", AggregateIntValue = N }).GetAwaiter().GetResult();
            }
        }

        private string GetMongoDbUrl()
            => "mongodb://" + new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["MongoDb_EventStore_Benchmarks:Server"];

        #endregion

        #region Public methods

        [Benchmark]
        public async Task StoreDomainEvent()
        {
            await new MongoDbEventStore().StoreDomainEventAsync(
                new TestEvent(Guid.NewGuid(), AggregateId)
                {
                    AggregateIntValue = 1,
                    AggregateStringValue = "test"
                });
        }

        [Benchmark]
        public async Task StoreRangeDomainEvent()
        {
            for (int i = 0; i < N; i++)
            {
                await new MongoDbEventStore().StoreDomainEventAsync(
                    new TestEvent(Guid.NewGuid(), AggregateId)
                    {
                        AggregateIntValue = 1,
                        AggregateStringValue = "test"
                    });
            }
        }


        [Benchmark]
        public async Task StoreRangeDomainEvent_Snapshot()
        {
            for (int i = 0; i < N; i++)
            {
                await new MongoDbEventStore(new BasicSnapshotBehaviorProvider(new Dictionary<Type, ISnapshotBehavior>()
                    { {typeof(TestEvent), new NumericSnapshotBehavior( 10) }})).StoreDomainEventAsync(
                   new TestEvent(Guid.NewGuid(), AggregateId)
                   {
                       AggregateIntValue = 1,
                       AggregateStringValue = "test"
                   });
            }
        }

        //[Benchmark]
        //public async Task GetEventsByAggregateId()
        //{
        //    var store = new MongoDbEventStore();
        //    for (int i = 0; i < N; i++)
        //    {
        //        if (i % 348 == 0)
        //        {
        //            await store.StoreDomainEventAsync(new TestEvent(Guid.NewGuid(), AggregateId));
        //        }
        //        else
        //        {
        //            await store.StoreDomainEventAsync(new TestEvent(Guid.NewGuid(), Guid.NewGuid()));
        //        }
        //    }
        //    var evt
        //        = await store.GetEventsFromAggregateIdAsync
        //        (
        //           AggregateId, typeof(TestAggregate)
        //        );
        //}

        [Benchmark]
        public async Task RehydrateAggregate()
        {
            var store = new MongoDbEventStore();
            var agg = await store.GetRehydratedAggregateAsync<TestAggregate, Guid>(AggregateId);

        }

        [Benchmark]
        public async Task RehydrateAggregate_WithSnapshot()
        {
            var store = new MongoDbEventStore(new BasicSnapshotBehaviorProvider(new Dictionary<Type, ISnapshotBehavior>()
                    { {typeof(TestEvent), new NumericSnapshotBehavior( 10) }}));
            var agg = await store.GetRehydratedAggregateAsync<TestAggregate, Guid>(AggregateId);

        }

        #endregion

    }
}
