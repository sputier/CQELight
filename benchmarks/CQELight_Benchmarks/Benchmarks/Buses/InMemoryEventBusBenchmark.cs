using BenchmarkDotNet.Attributes;
using CQELight.Buses.InMemory.Commands;
using CQELight.Buses.InMemory.Events;
using CQELight_Benchmarks.Models;
using System;
using System.Collections.Generic;
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
        public int JobDuration { get; set; }

        #endregion

        #region Public methods
        
        [Benchmark] // We add those benchmark that are mainly the same as command cause we must not see any perf differences between these two
        public async Task DispatchEvent()
        {
            var bus = new InMemoryEventBus();
            await bus.RegisterAsync(new TestDispatchEvent(0, SimulateWork, JobDuration));
        }

        [Benchmark]
        [Arguments(10, false)]
        [Arguments(10, true)]
        [Arguments(100, false)]
        [Arguments(100, true)]
        [Arguments(1000, true)]
        public async Task DispatchRangeEvents(int nbEvents, bool allowParallel)
        {
            var bus = new InMemoryEventBus(new InMemoryEventBusConfiguration
            {
                _parallelDispatch = allowParallel ? new List<Type> { typeof(TestDispatchEvent) } : new List<Type>()
            });
            for (int i = 0; i < nbEvents; i++)
            {
                await bus.RegisterAsync(new TestDispatchEvent(i, SimulateWork, JobDuration));
            }
        }

        #endregion

    }
}
