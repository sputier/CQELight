using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Jobs;
using CQELight;
using CQELight.Dispatcher;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight_Benchmarks.Benchmarks
{
    [CoreJob]
    [RankColumn]
    public class CosmosDbBenchmarks : EventStoreBaseBenchmark
    {

        #region BenchmarkDotNet

        [GlobalSetup]
        public void Setup()
        {
            new Bootstrapper()
                .UseCosmosDbAsEventStore(
                    "https://localhost:8081", 
                    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==")
                .Bootstrapp();
        }

        #endregion
        

    }
}
