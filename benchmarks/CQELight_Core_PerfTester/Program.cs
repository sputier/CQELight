using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.InMemory.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CQELight_Core_PerfTester
{

    class TestEvent : BaseDomainEvent
    {
        public TestEvent(int eventNumber, bool simulateWork, int workduration)
        {
            EventNumber = eventNumber;
            SimulateWork = simulateWork;
            Workduration = workduration;
        }

        public int EventNumber { get; }
        public bool SimulateWork { get; }
        public int Workduration { get; }
    }

    class TestEventHandler : IDomainEventHandler<TestEvent>
    {
        public async Task HandleAsync(TestEvent domainEvent, IEventContext context = null)
        {
            if(domainEvent.SimulateWork)
            {
                await Task.Delay(domainEvent.Workduration);
            }
        }
    }
    static class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing InMemory EventBus (y/n) ? ");
            var result = Console.ReadKey().Key;
            if (result == ConsoleKey.Y)
            {
                Console.WriteLine("Jitting now...");

                var bus = new InMemoryEventBus();
                await bus.PublishEventAsync(new TestEvent(-1, false, 0));

                Console.WriteLine("Testing 250 events WITHOUT parallel dispatch WITHOUT work simulation");
                for (int i = 0; i < 250; i++)
                {
                    await bus.PublishEventAsync(new TestEvent(i, false, 0));
                }

                Console.WriteLine("Testing 250 events WITHOUT parallel dispatch WITH 100 ms work simulation");
                for (int i = 0; i < 250; i++)
                {
                    await bus.PublishEventAsync(new TestEvent(i, true, 100));
                }

                Console.WriteLine("Testing 250 events WITH parallel dispatch WITH 100 ms work simulation");

                var tasks = new List<Task>();
                for (int i = 0; i < 250; i++)
                {
                    tasks.Add(bus.PublishEventAsync(new TestEvent(i, true, 100)));
                }
                await Task.WhenAll(tasks);
            }

        }
    }
}
