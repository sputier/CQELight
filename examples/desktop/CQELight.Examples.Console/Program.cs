using CQELight.Buses.InMemory;
using CQELight.Buses.InMemory.Commands;
using CQELight.Buses.InMemory.Events;
using CQELight.DAL.EFCore;
using CQELight.Dispatcher;
using CQELight.Dispatcher.Configuration;
using CQELight.Events.Serializers;
using CQELight.Examples.Console.Commands;
using CQELight.EventStore.EFCore;
using CQELight.IoC.Autofac;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CQELight.Examples.Console
{
    /**
     *  This program is an example to show how can CQELight be used in 
     *  a single process, like a console app, WPF, WinForms app or Xamarin App.
     * 
     *  The demonstration here are used to show the ability of adding more features 
     *  when application has been finalized for the first time.
     * 
     *  
     * 
     */
    class Program
    {
        static async Task Main(string[] args)
        {

            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.WriteLine("System initializing...");
            System.Console.ForegroundColor = ConsoleColor.White;

            new Bootstrapper()
                .ConfigureDispatcher(GetCoreDispatcherConfiguration())
                .UseInMemoryEventBus(new InMemoryEventBusConfiguration(3, 250, (evt, ctx) => { }))
                .UseInMemoryCommandBus(new InMemoryCommandBusConfiguration((cmd, ctx) => { }))
                .UseEFCoreAsMainRepository(new AppDbContext())
                .UseSQLServerWithEFCoreAsEventStore(Consts.CONST_CONNECTION_STRING)
                .UseAutofacAsIoC(c => { });

            using (var ctx = new AppDbContext())
            {
                await ctx.Database.MigrateAsync().ConfigureAwait(false);
            }

            System.Console.WriteLine("Message manager");
            System.Console.WriteLine("Enter '/q' to exit system");

            string message = string.Empty;
            while (true)
            {
                System.Console.WriteLine("Enter your message to be proceed by system : ");
                message = System.Console.ReadLine();
                if (message == "/q")
                {
                    break;
                }
                else if(!string.IsNullOrWhiteSpace(message))
                {
                    await CoreDispatcher.DispatchCommandAsync(new SendMessageCommand(message)).ConfigureAwait(false);
                }
            }
        }

        private static CoreDispatcherConfiguration GetCoreDispatcherConfiguration()
        {
            var configurationBuilder = new CoreDispatcherConfigurationBuilder();
            configurationBuilder
                .ForAllEvents()
                .UseBus<InMemoryEventBus>().HandleErrorWith(e =>
                {
                    System.Console.ForegroundColor = ConsoleColor.DarkRed;
                    System.Console.WriteLine("An error has been raised during dispatch : " + e);
                    System.Console.ForegroundColor = ConsoleColor.White;
                })
                .SerializeWith<JsonEventSerializer>();
            return configurationBuilder.Build();
        }
    }
}
