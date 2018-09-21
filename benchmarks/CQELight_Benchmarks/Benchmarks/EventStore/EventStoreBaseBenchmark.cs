using BenchmarkDotNet.Attributes;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Dispatcher;
using CQELight.EventStore;
using CQELight.EventStore.MongoDb;
using CQELight.EventStore.MongoDb.Snapshots;
using CQELight_Benchmarks.Custom;
using CQELight_Benchmarks.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks.Benchmarks
{
    public class EventStoreBaseBenchmark : BaseBenchmark
    {

        #region Members
        
        private Guid _snapshotedAggId;

        #endregion

        #region BenchmarkDotNet

        [GlobalSetup]
        public void GlobalSetup()
        {
            AggregateId = Guid.NewGuid();
            _snapshotedAggId = Guid.NewGuid();
        }

        #endregion

        #region Public methods

        [Benchmark]
        [BenchmarkOrder(1)]
        public async Task PublishAndSaveEvents_Simple()
        {
            var evtId = Guid.NewGuid();
            await CoreDispatcher.PublishEventAsync(
                new BenchmarkSimpleEvent(evtId)
                {
                    IntValue = N,
                    StringValue = N.ToString(),
                    DateTimeValue = DateTime.Today
                }).ConfigureAwait(false);
        }

        [Benchmark]
        [BenchmarkOrder(2)]
        public async Task PublishAndSaveEvents_Aggregate()
        {
            await CoreDispatcher.PublishEventAsync(
                 new TestEvent(Guid.NewGuid(), AggregateId)
                 {
                     AggregateIntValue = N,
                     AggregateStringValue = N.ToString()
                 }).ConfigureAwait(false);
        }

        [Benchmark, BenchmarkOrder(3)]
        public async Task GetEventsByAggregateId()
        {
            var evt
                = await new MongoDbEventStore().GetEventsFromAggregateIdAsync<BenchmarkSimpleEvent>
                (
                   AggregateId
                ).ConfigureAwait(false);
        }

        [Benchmark, BenchmarkOrder(4)]
        public async Task RehydrateAggregate()
        {
            var agg =
                await new MongoDbEventStore().GetRehydratedAggregateAsync<TestAggregate>(AggregateId).ConfigureAwait(false);
        }

        [Benchmark, BenchmarkOrder(5)]
        public async Task StoreEvent_RehydrateAggregate_WithSnapshot()
        {
            var agg =
                await new MongoDbEventStore(
                    new BasicSnapshotBehaviorProvider(
                        new Dictionary<Type, ISnapshotBehavior>() { { typeof(TestEvent), new NumericSnapshotBehavior(10) } }))
                        .GetRehydratedAggregateAsync<TestAggregate>(AggregateId).ConfigureAwait(false);
        }

        #endregion

    }
}
