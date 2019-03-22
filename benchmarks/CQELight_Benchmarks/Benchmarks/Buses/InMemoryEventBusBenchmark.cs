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

        [GlobalSetup]
        public void GlobalSetupA()
        {
            InMemoryEventBus.InitHandlersCollection(new string[0]);
        }

        [Params(10, 25, 50)]
        public int MillisecondsJobDuration { get; set; }

        #endregion

        #region Public methods

        [Benchmark]
        public async Task PublishSingleEvent()
        {
            var bus = new InMemoryEventBus();
            await bus.PublishEventAsync(new TestDispatchEvent(0, MillisecondsJobDuration != 0, MillisecondsJobDuration));
        }

        [Benchmark]
        [Arguments(10)]
        [Arguments(20)]
        [Arguments(50)]
        public async Task PublishEventRange_SameEventType_SameAggId_NoParallel(int nbEvents)
        {
            var bus = new InMemoryEventBus();
            var events = new List<IDomainEvent>();
            Guid aggId = Guid.NewGuid();
            for (int i = 0; i < nbEvents; i++)
            {
                events.Add(new TestDispatchEvent(i, MillisecondsJobDuration != 0, MillisecondsJobDuration,
                    aggId, typeof(object)));
            }
            await bus.PublishEventRangeAsync(events);
        }

        [Benchmark]
        [Arguments(10)]
        [Arguments(20)]
        [Arguments(50)]
        public async Task PublishEventRange_SameEventType_SameAggId_ParallelDispatch(int nbEvents)
        {
            var bus = new InMemoryEventBus(new InMemoryEventBusConfiguration
            {
                _parallelDispatch = new List<Type> { typeof(TestDispatchEvent) }
            });
            var events = new List<IDomainEvent>();
            Guid aggId = Guid.NewGuid();
            for (int i = 0; i < nbEvents; i++)
            {
                events.Add(new TestDispatchEvent(i, MillisecondsJobDuration != 0, MillisecondsJobDuration,
                    aggId, typeof(object)));
            }
            await bus.PublishEventRangeAsync(events);
        }

        [Benchmark]
        [Arguments(50)]
        [Arguments(100)]
        [Arguments(250)]
        public async Task PublishEventRange_SameEventType_SameAggId_ParallelHandling(int nbEvents)
        {
            var bus = new InMemoryEventBus(new InMemoryEventBusConfiguration
            {
                _parallelHandling = new List<Type> { typeof(TestDispatchEvent) }
            });
            var events = new List<IDomainEvent>();
            Guid aggId = Guid.NewGuid();
            for (int i = 0; i < nbEvents; i++)
            {
                events.Add(new TestDispatchEvent(i, MillisecondsJobDuration != 0, MillisecondsJobDuration,
                    aggId, typeof(object)));
            }
            await bus.PublishEventRangeAsync(events);
        }

        [Benchmark]
        [Arguments(50)]
        [Arguments(100)]
        [Arguments(250)]
        public async Task PublishEventRange_SameEventType_SameAggId_AllParallel(int nbEvents)
        {
            var bus = new InMemoryEventBus(new InMemoryEventBusConfiguration
            {
                _parallelHandling = new List<Type> { typeof(TestDispatchEvent) },
                _parallelDispatch = new List<Type> { typeof(TestDispatchEvent) }
            });
            var events = new List<IDomainEvent>();
            Guid aggId = Guid.NewGuid();
            for (int i = 0; i < nbEvents; i++)
            {
                events.Add(new TestDispatchEvent(i, MillisecondsJobDuration != 0, MillisecondsJobDuration,
                    aggId, typeof(object)));
            }
            await bus.PublishEventRangeAsync(events);
        }

        [Benchmark]
        [Arguments(50)]
        [Arguments(100)]
        [Arguments(250)]
        public async Task PublishEventRange_SameEventType_TwoGroupsAggId_AllParallel(int nbEvents)
        {
            var bus = new InMemoryEventBus(new InMemoryEventBusConfiguration
            {
                _parallelHandling = new List<Type> { typeof(TestDispatchEvent) },
                _parallelDispatch = new List<Type> { typeof(TestDispatchEvent) }
            });
            var events = new List<IDomainEvent>();
            Guid aggId = Guid.NewGuid();
            Guid aggId2 = Guid.NewGuid();
            for (int i = 0; i < nbEvents; i++)
            {
                events.Add(new TestDispatchEvent(i, MillisecondsJobDuration != 0, MillisecondsJobDuration,
                    i % 2 == 0 ? aggId : aggId2, typeof(object)));
            }
            await bus.PublishEventRangeAsync(events);
        }

        [Benchmark]
        [Arguments(50)]
        [Arguments(100)]
        [Arguments(250)]
        public async Task PublishEventRange_SameEventType_AllDifferentAggId_AllParallel(int nbEvents)
        {
            var bus = new InMemoryEventBus(new InMemoryEventBusConfiguration
            {
                _parallelHandling = new List<Type> { typeof(TestDispatchEvent) },
                _parallelDispatch = new List<Type> { typeof(TestDispatchEvent) }
            });
            var events = new List<IDomainEvent>();
            for (int i = 0; i < nbEvents; i++)
            {
                events.Add(new TestDispatchEvent(i, MillisecondsJobDuration != 0, MillisecondsJobDuration,
                    Guid.NewGuid(), typeof(object)));
            }
            await bus.PublishEventRangeAsync(events);
        }

        #endregion

    }
}
