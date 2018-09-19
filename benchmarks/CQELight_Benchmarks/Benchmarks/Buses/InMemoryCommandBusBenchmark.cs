using BenchmarkDotNet.Attributes;
using CQELight.Buses.InMemory.Commands;
using CQELight_Benchmarks.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks.Benchmarks
{
    class InMemoryCommandBusBenchmark
    {
        #region BenchmarkDotNet

        [Params(true, false)]
        public bool SimulateWork { get; set; }
        [Params(100, 250, 500)]
        public int JobDuration { get; set; }

        #endregion

        #region Public methods

        [Benchmark]
        public async Task DispatchACommand()
        {
            var bus = new InMemoryCommandBus();
            await bus.DispatchAsync(new TestCommand(0, SimulateWork, JobDuration));
        }

        [Benchmark]
        [Arguments(10)]
        [Arguments(100)]
        [Arguments(1000)]
        [Arguments(10_000)]
        public async Task DispatchRangeCommands(int nbCommands)
        {
            var bus = new InMemoryCommandBus();
            for (int i = 0; i < nbCommands; i++)
            {
                await bus.DispatchAsync(new TestCommand(i, SimulateWork, JobDuration));
            }
        }

        #endregion

    }
}
