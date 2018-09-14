using BenchmarkDotNet.Attributes;
using CQELight;
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
            new Bootstrapper().UseMongoDbAsEventStore(new MongoDbEventStoreBootstrapperConfiguration(GetMongoDbUrl())).Bootstrapp();
        }

        [GlobalSetup(Targets = new[] { nameof(RehydrateAggregate_NoSnapshot) })]
        public void GlobalSetup_Storage()
        {
            Console.WriteLine("Aggregate Id : " + AggregateId.ToString());
            StoreNDomainEvents();
        }

        [GlobalSetup(Targets = new[] { nameof(RehydrateAggregate_WithSnapshot) })]
        public void GlobalSetup_Storage_Snapshot()
        {
            Console.WriteLine("Aggregate Id : " + AggregateId.ToString());
            StoreNDomainEvents(new BasicSnapshotBehaviorProvider(new Dictionary<Type, ISnapshotBehavior>()
            {
                {typeof(TestEvent), new NumericSnapshotBehavior( 10) }
            }));
        }

        //[IterationSetup(Targets = new[] { nameof(StoreDomainEvent), nameof(StoreDomainEvent_Aggregate), nameof(GetEventsByAggregateId) })]
        //public void DropDatabases()
        //{
        //    var client = new MongoClient(GetMongoDbUrl());
        //    client.DropDatabase("CQELight_Events");
        //}

        #endregion

        #region Private methods

        private void StoreNDomainEvents(ISnapshotBehaviorProvider provider = null)
        {
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

        //[Benchmark]
        //public void StoreDomainEvent()
        //{
        //    new MongoDbEventStore().StoreDomainEventAsync(
        //        new BenchmarkSimpleEvent(Guid.NewGuid())
        //        {
        //            IntValue = 1,
        //            StringValue = "test",
        //            DateTimeValue = DateTime.Today
        //        }).GetAwaiter().GetResult();
        //}

        //[Benchmark]
        //public void StoreDomainEvent_Aggregate()
        //{
        //    new MongoDbEventStore().StoreDomainEventAsync(
        //         new TestEvent(Guid.NewGuid(), AggregateId)
        //         {
        //             AggregateIntValue = 1,
        //             AggregateStringValue = "test"
        //         }).GetAwaiter().GetResult();
        //}

        //[Benchmark]
        //public void GetEventsByAggregateId()
        //{
        //    var evt
        //        = new MongoDbEventStore().GetEventsFromAggregateIdAsync<BenchmarkSimpleEvent>
        //        (
        //           AggregateId
        //        ).GetAwaiter().GetResult();
        //}

        [Benchmark]
        public void RehydrateAggregate_NoSnapshot()
        {
            var agg =
                new MongoDbEventStore().GetRehydratedAggregateAsync<TestAggregate>(AggregateId).GetAwaiter().GetResult();

        }

        [Benchmark]
        public void RehydrateAggregate_WithSnapshot()
        {
            var agg =
                new MongoDbEventStore(
                    new BasicSnapshotBehaviorProvider(
                        new Dictionary<Type, ISnapshotBehavior>() { { typeof(TestEvent), new NumericSnapshotBehavior(10) } }))
                        .GetRehydratedAggregateAsync<TestAggregate>(AggregateId).GetAwaiter().GetResult();

        }

        #endregion

    }
}
