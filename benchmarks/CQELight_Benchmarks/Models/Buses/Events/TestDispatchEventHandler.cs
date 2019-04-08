using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks.Models
{
    public class TestDispatchEventHandler : IDomainEventHandler<TestDispatchEvent>
    {
        public async Task<Result> HandleAsync(TestDispatchEvent domainEvent, IEventContext context = null)
        {
            if (domainEvent.JobDuration != 0)
            {
                await Task.Delay(domainEvent.JobDuration); //Simulation of max 500ms job here
            }
            return Result.Ok();
        }
    }
}
