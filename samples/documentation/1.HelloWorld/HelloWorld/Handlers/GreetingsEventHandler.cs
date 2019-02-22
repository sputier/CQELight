using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using HelloWorld.Events;
using System;
using System.Threading.Tasks;

namespace HelloWorld.Handlers
{
    class GreetingsEventHandler : IDomainEventHandler<GreetingsEvent>
    {
        public Task<Result> HandleAsync(GreetingsEvent domainEvent, IEventContext context = null)
        {
            Console.WriteLine("Hello world!");
            return Task.FromResult(Result.Ok());
        }
    }
}
