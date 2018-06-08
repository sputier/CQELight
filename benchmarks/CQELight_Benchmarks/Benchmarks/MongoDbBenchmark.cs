using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using CQELight;
using CQELight.Dispatcher;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks.Benchmarks
{
    [CoreJob]
    [RankColumn]
    public class MongoDbBenchmark : BaseBenchmark
    {

        #region BenchmarkDotNet

        [GlobalSetup]
        public void Setup()
        {
            new Bootstrapper()
                .UseMongoDbAsEventStore("mongodb://127.0.0.1")
                .Bootstrapp();
        }


        #endregion

        #region Public methods

        [Benchmark]
        public Task PublishAndSaveEvents()
        {
            return CoreDispatcher.PublishEventAsync(
                new BenchmarkSimpleEvent(Guid.NewGuid()) { IntValue = N, StringValue = N.ToString(), DateTimeValue = DateTime.Today });
        }

        #endregion
    }
}
