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
    [ClrJob, CoreJob]
    [RPlotExporter, RankColumn]
    public class MongoDbBenchmark : BaseBenchmark
    {

        #region BenchmarkDotNet

        public BenchmarkSimpleEvent[] Events;

        [GlobalSetup]
        public void Setup()
        {
            Events = new BenchmarkSimpleEvent[N];
            for(int i = 0; i < N; i ++)
            {
                Events[i] = new BenchmarkSimpleEvent(Guid.NewGuid()) { IntValue = i, StringValue = i.ToString(), DateTimeValue = DateTime.Today.AddDays(-i) };
            }
        }


        #endregion

        #region Public methods

        [Benchmark]
        public Task PublishAndSaveEvents()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < N; i++)
            {
                tasks.Add(CoreDispatcher.PublishEventAsync(Events[i]));
            }
            return Task.WhenAll(tasks);
        }

        #endregion
    }
}
