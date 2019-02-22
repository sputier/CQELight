using BenchmarkDotNet.Attributes;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.InMemory.Commands;
using CQELight.Buses.InMemory.Events;
using CQELight_Benchmarks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks.Benchmarks
{
    public class InMemoryEventBusBenchmark
    {

        #region BenchmarkDotNet

        [Params(true, false)]
        public bool SimulateWork { get; set; }

        [Params(100, 250, 500)]
        public int MillisecondsJobDuration { get; set; }

        #endregion

        #region Public methods

        //[Benchmark] // We add those benchmark that are mainly the same as command cause we must not see any perf differences between these two
        //public async Task DispatchEvent()
        //{
        //    var bus = new InMemoryEventBus();
        //    await bus.PublishEventAsync(new TestDispatchEvent(0, SimulateWork, MillisecondsJobDuration));
        //}

        [Benchmark]
        [Arguments(50, false, false)]
        [Arguments(50, false, true)]
        [Arguments(50, true, false)]
        [Arguments(50, true, true)]
        [Arguments(100, false, false)]
        [Arguments(100, true, false)]
        [Arguments(100, false, true)]
        [Arguments(100, true, true)]
        [Arguments(250, true, true)]
        [Arguments(1000, true, true)]
        [Arguments(2500, true, true)]
        public async Task DispatchRangeEvents_SameEvent(int nbEvents, bool allowParallelDispatch, bool allowParallelHandling)
        {
            var bus = new InMemoryEventBus(new InMemoryEventBusConfiguration
            {
                _parallelDispatch = allowParallelDispatch ? new List<Type> { typeof(TestDispatchEvent) } : new List<Type>(),
                _parallelHandling = allowParallelHandling ? new List<Type> { typeof(TestDispatchEvent) } : new List<Type>()
            });
            var events = new List<IDomainEvent>();
            for (int i = 0; i < nbEvents; i++)
            {
                events.Add(new TestDispatchEvent(i, SimulateWork, MillisecondsJobDuration));
            }
            await bus.PublishEventRangeAsync(events);
        }

        #endregion

    }
}
