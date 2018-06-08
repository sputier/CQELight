using BenchmarkDotNet.Attributes;
using CQELight.Dispatcher;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight_Benchmarks.Benchmarks
{
    public class EventStoreBaseBenchmark : BaseBenchmark
    {

        #region Public methods

        [Benchmark]
        public void PublishAndSaveEvents()
        {
            CoreDispatcher.PublishEventAsync(
                new BenchmarkSimpleEvent(Guid.NewGuid()) { IntValue = N, StringValue = N.ToString(), DateTimeValue = DateTime.Today })
                .GetAwaiter().GetResult();
        }

        #endregion

    }
}
