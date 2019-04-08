using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight_Benchmarks.Models
{
    public class TestEventHandler : IDomainEventHandler<TestEvent>
    {
        public Task<Result> HandleAsync(TestEvent domainEvent, IEventContext context = null)
        {
            return Task.FromResult(Result.Ok());
        }
    }
}
