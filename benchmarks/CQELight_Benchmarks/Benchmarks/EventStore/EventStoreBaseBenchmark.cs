using BenchmarkDotNet.Attributes;
using CQELight.Dispatcher;
using CQELight.EventStore.MongoDb;
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

        private Guid _aggId;

        #endregion

        #region BenchmarkDotNet

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine("//**** EVENT STORE GLOBAL SETUP ****//");
            _aggId = Guid.NewGuid();
            GlobalSetupSpec();
        }

        public virtual void GlobalSetupSpec()
        { }

        #endregion

        #region Public methods

        [Benchmark]
        [BenchmarkOrder(1)]
        public async Task PublishAndSaveEvents_Simple()
        {
            var evtId = Guid.NewGuid();
            Program.AggregateIds.Add(evtId);
            await CoreDispatcher.PublishEventAsync(
                new BenchmarkSimpleEvent(evtId)
                {
                    IntValue = N,
                    StringValue = N.ToString(),
                    DateTimeValue = DateTime.Today
                });
        }

        [Benchmark]
        [BenchmarkOrder(2)]
        public async Task PublishAndSaveEvents_Aggregate()
        {
            await CoreDispatcher.PublishEventAsync(
                 new AggregateEvent(Guid.NewGuid(), _aggId)
                 {
                     AggregateIntValue = N,
                     AggregateStringValue = N.ToString()
                 });
        }

        [Benchmark, BenchmarkOrder(3)]
        public async Task GetEventById()
        {
            var evt
                = await new MongoDbEventStore().GetEventByIdAsync<BenchmarkSimpleEvent>
                (
                        Program.AggregateIds[_random.Next(0, Program.AggregateIds.Count - 1)]
                );
        }

        [Benchmark, BenchmarkOrder(4)]
        public async Task GetEventsByAggregateId()
        {
            var evt
                = await new MongoDbEventStore().GetEventsFromAggregateIdAsync<BenchmarkSimpleEvent>
                (
                   _aggId
                );
        }

        #endregion

    }
}
