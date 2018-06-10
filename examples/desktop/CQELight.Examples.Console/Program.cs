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
using CQELight.Tools.Extensions;
using System.Collections.Generic;
using CQELight.Bootstrapping.Notifications;

namespace CQELight.Examples.Console
{
    /**
     *  This program is an example to show how can CQELight be used in 
     *  a single process, like a console app, WPF, WinForms app or Xamarin App.
     * 
     *  The demonstration here are used to show the ability of adding more features 
     *  when application has been finalized for the first time. 
     */
    internal static class Program
    {
        #region Main

        private static async Task Main(string[] args)
        {
            bool automaticConfig = ProgramMenus.DrawChoiceMenu();

            if (automaticConfig)
            {
                System.Console.ForegroundColor = ConsoleColor.DarkGreen;
                System.Console.WriteLine("Automatic configuration will be applied with the following : ");
                System.Console.WriteLine("\tDispatcher will show error in console");
                System.Console.WriteLine("\tInMemory events & commands bus will be used for anything");
                System.Console.WriteLine("\tEF Core with SQL Server will be used as repositories");
                System.Console.WriteLine("\tEF Core with SQL Server will be used as event store");
                System.Console.WriteLine("\tAutofac will be used as ioc container");
                System.Console.WriteLine("System is initializing, please wait ...");
                System.Console.ForegroundColor = ConsoleColor.White;

                new Bootstrapper()
                   .ConfigureCoreDispatcher(GetCoreDispatcherConfiguration())
                   .UseInMemoryEventBus(GetInMemoryEventBusConfiguration())
                   .UseInMemoryCommandBus()
                   .UseEFCoreAsMainRepository(new AppDbContext())
                   .UseSQLServerWithEFCoreAsEventStore(Consts.CONST_EVENT_DB_CONNECTION_STRING)
                   .UseAutofacAsIoC(c => { })
                   .Bootstrapp();
            }
            else
            {
                ProgramMenus.DrawConfigurationMenu();
            }

            System.Console.Clear();

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
                else if (!string.IsNullOrWhiteSpace(message))
                {
                    await CoreDispatcher.DispatchCommandAsync(new SendMessageCommand(message)).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Private static methods

        private static InMemoryEventBusConfiguration GetInMemoryEventBusConfiguration()
            => new InMemoryEventBusConfigurationBuilder()
                .SetRetryStrategy(250, 3)
                .Build();

        private static DispatcherConfiguration GetCoreDispatcherConfiguration()
        {
            var configurationBuilder = new CoreDispatcherConfigurationBuilder();
            configurationBuilder
                .ForAllEvents()
                .UseBus<InMemoryEventBus>()
                .HandleErrorWith(e =>
                {
                    System.Console.ForegroundColor = ConsoleColor.DarkRed;
                    System.Console.WriteLine("An error has been raised during event dispatch : " + e);
                    System.Console.ForegroundColor = ConsoleColor.White;
                })
                .SerializeWith<JsonDispatcherSerializer>();
            configurationBuilder
                .ForAllCommands()
                .UseBus<InMemoryCommandBus>()
                .HandleErrorWith(e =>
                {
                    System.Console.ForegroundColor = ConsoleColor.DarkRed;
                    System.Console.WriteLine("An error has been raised during command dispatch : " + e);
                    System.Console.ForegroundColor = ConsoleColor.White;
                })
                .SerializeWith<JsonDispatcherSerializer>();
            return configurationBuilder.Build();
        }

        #endregion

    }
}
