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
    public class MongoDbBenchmarks : EventStoreBaseBenchmark
    {

        #region EventStoreBaseBenchmark

        public override void GlobalSetupSpec()
        {
            new Bootstrapper()
                .UseMongoDbAsEventStore("mongodb://127.0.0.1")
                .Bootstrapp();
        }


        #endregion

       
    }
}
