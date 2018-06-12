using BenchmarkDotNet.Attributes;
using CQELight.Dispatcher;
using CQELight.EventStore.MongoDb;
using CQELight_Benchmarks.Custom;
using CQELight_Benchmarks.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        public void PublishAndSaveEvents_Simple()
        {
            var evtId = Guid.NewGuid();
            File.Create(Consts.CONST_EVT_IDS_DIR + evtId.ToString() + ".g").Close();
            CoreDispatcher.PublishEventAsync(
                new BenchmarkSimpleEvent(evtId)
                {
                    IntValue = N,
                    StringValue = N.ToString(),
                    DateTimeValue = DateTime.Today
                })
                .GetAwaiter().GetResult();
        }

        [Benchmark]
        [BenchmarkOrder(2)]
        public void PublishAndSaveEvents_Aggregate()
        { 
            CoreDispatcher.PublishEventAsync(
                new AggregateEvent(Guid.NewGuid(), _aggId)
                {
                    AggregateIntValue = N,
                    AggregateStringValue = N.ToString()
                })
                .GetAwaiter().GetResult();
        }

        [Benchmark, BenchmarkOrder(3)]
        public void GetEventById()
        {
            var allIds = Directory.GetFiles(Consts.CONST_EVT_IDS_DIR);
            var evt
                = new MongoDbEventStore().GetEventByIdAsync<BenchmarkSimpleEvent>
                (
                    Guid.Parse(
                        allIds[_random.Next(0, allIds.Length - 1)]
                        .Replace(Consts.CONST_EVT_IDS_DIR, "")
                        .Replace(".g", "").Trim())
                )
                .GetAwaiter().GetResult();
        }

        [Benchmark, BenchmarkOrder(4)]
        public void GetEventsByAggregateId()
        {
            var evt
                = new MongoDbEventStore().GetEventsFromAggregateIdAsync<BenchmarkSimpleEvent>
                (
                   _aggId
                )
                .GetAwaiter().GetResult();
        }

        #endregion

    }
}
