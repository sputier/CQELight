using CQELight;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Dispatcher;
using CQELight.Tools;
using HelloWorld.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HelloWorld
{
    static class Program
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
