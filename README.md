# CQELight

[![Build Status](https://dev.azure.com/hybrid-technologies-solutions/CQELight_CI/_apis/build/status/CQELight-CI?branchName=develop)](https://dev.azure.com/hybrid-technologies-solutions/CQELight_CI/_build/latest?definitionId=7&branchName=develop)
[![Documentation Status](https://readthedocs.org/projects/cqelight/badge/?version=latest)](https://cqelight.readthedocs.io/en/latest/?badge=latest)

## Description 
CQELight is a DDD, Command Query & Event Sourcing extensible and customisable base framework

DDD, CQRS and Event-sourcing are great topics, but it's not always easy to get started with them. Here's where CQELight is.

CQELight allows you to do clean loosely coupled architecture for your software developpments. Like this, you won't have to worry about technical stuff, just focus on business stuff and rely on CQELight system to help you build and run your system.

Based on Domain Driven Design, you can create your objects within boundaries, as aggregates, entities or value objects.
With this clean object architecture, you can perform simple, flexible and extensible CQRS operations for interact with the system.

Available packages : 

Extension name                             | Stable                      
-------------------------------------------|-----------------------------
CQELight base| [![NuGet](https://img.shields.io/nuget/v/CQELight.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CQELight/)
InMemory Buses | [![NuGet](https://img.shields.io/nuget/v/CQELight.Buses.InMemory.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CQELight.Buses.InMemory/)
RabbitMQ Bus | [![NuGet](https://img.shields.io/nuget/v/CQELight.Buses.RabbitMQ.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CQELight.Buses.RabbitMQ/)
MSMQ Bus | [![NuGet](https://img.shields.io/nuget/v/CQELight.Buses.MSMQ.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CQELight.Buses.MSMQ/)
Azure Service Bus | [![NuGet](https://img.shields.io/nuget/v/CQELight.Buses.AzureServiceBus.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CQELight.Buses.AzureServiceBus/)
Autofac IoC | [![NuGet](https://img.shields.io/nuget/v/CQELight.IoC.Autofac.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CQELight.IoC.Autofac/)
MongoDb EventStore | [![NuGet](https://img.shields.io/nuget/v/CQELight.EventStore.MongoDb.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CQELight.EventStore.MongoDb/)
EF Core EventStore | [![NuGet](https://img.shields.io/nuget/v/CQELight.EventStore.EFCore.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CQELight.EventStore.EFCore/)
EF Core DAL | [![NuGet](https://img.shields.io/nuget/v/CQELight.DAL.EFCore.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CQELight.DAL.EFCore/)
TestFramework | [![NuGet](https://img.shields.io/nuget/v/CQELight.TestFramework.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CQELight.TestFramework/)|
MVVM | [![NuGet](https://img.shields.io/nuget/v/CQELight.MVVM.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CQELight.MVVM/)|
MVVM - MahApps implementation | [![NuGet](https://img.shields.io/nuget/v/CQELight.MVVM.MahApps.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/CQELight.MVVM.MahApps/)

## Quick getting started - The 'Hello World!' example

To get really quick started, create a new console application

`dotnet new console`

Edit your csproj to use latest C# version

```
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.1</TargetFramework>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

</Project>
```

Add CQELight & CQELight.Buses.InMemory packages

`dotnet add package CQELight | dotnet add package CQELight.Buses.InMemory` 

Create a new class GreetingsEvent.cs and add the following content

```csharp
using CQELight.Abstractions.Events;
namespace HelloWorld.Events
{
    class GreetingsEvent : BaseDomainEvent
    {
    }
}
```

Create a new class GreetingsEventHandler.cs and add the following content 

```csharp
using CQELight.Abstractions.Events.Interfaces;
using HelloWorld.Events;
using System;
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

```csharp
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
            new Bootstrapper()
                .UseInMemoryEventBus()
                .Bootstrapp();

            await CoreDispatcher.PublishEventAsync(new GreetingsEvent()).ConfigureAwait(false);

            Console.Read();
        }
    }
}
```

Then, execute `dotnet run`, Hello World! should be visible on console
