using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks.Models
{
    public class TestDispatchEventHandler : IDomainEventHandler<TestDispatchEvent>
    {
        public async Task HandleAsync(TestDispatchEvent domainEvent, IEventContext context = null)
        {
            if (domainEvent.SimulateWork)
            {
                await Task.Delay(domainEvent.I % domainEvent.JobDuration); //Simulation of max 500ms job here
            }
        }
    }
}
