# CQELight

## Description 
CQELight is a DDD, Command Query & Event Sourcing extensible and customisable base framework

DDD, CQRS and Event-sourcing are great topics, but it's not always easy to get started with them. Here's where CQELight is.

CQELight allows you to do clean loosely coupled architecture for your software developpments. Like this, you won't have to worry about technical stuff, just focus on business stuff and rely on CQELight system to help you build and run your system.

Based on Domain Driven Design, you can create your objects within boundaries, as aggregates, entities or value objects.
With this clean object architecture, you can perform simple, flexible and extensible CQRS operations for interact with the system.

## Quick getting started - The 'Hello World!' example

To get really quick started, create a new console application

`dotnet new console`

Add CQELight & CQELight.Buses.InMemory packages

`dotnet add package CQELight | dotnet add package CQELight.Buses.InMemory` 

Create a new class GreetingsEvent.cs and add the following content

```
using CQELight.Abstractions.Events;
namespace HelloWorld.Events
{
    class GreetingsEvent : BaseDomainEvent
    {
    }
}
```

Create a new class GreetingsEventHandler.cs and add the following content 

```
using CQELight.Abstractions.Events.Interfaces;
using HelloWorld.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld.Handlers
{
    class GreetingsEventHandler : IDomainEventHandler<GreetingsEvent>
    {
        public Task HandleAsync(GreetingsEvent domainEvent, IEventContext context = null)
        {
            Console.WriteLine("Hello world!");
            return Task.CompletedTask;
        }
    }
}
```

Modify Program.cs as following

```
using CQELight;
using CQELight.Dispatcher;
using HelloWorld.Events;
using System;
using System.Threading.Tasks;

namespace HelloWorld
{
    class Program
    {
        static async Task Main(string[] args)
        {
            new Bootstrapper().
                UseInMemoryEventBus()
                .Bootstrapp();

            await CoreDispatcher.PublishEventAsync(new GreetingsEvent()).ConfigureAwait(false);

            Console.Read();
        }
    }
}
```

Then, execute `dotnet run`, Hello World! should be visible on console

## How do I get it? 

See our examples to discover all you can do with CQELight!

Find all packages on nuget : https://www.nuget.org/packages?q=cqelight
