using CQELight.Buses.InMemory.Events;
using CQELight.Dispatcher;
using CQELight.Dispatcher.Configuration;
using CQELight.Events.Serializers;
using CQELight.Examples.Console.Commands;
using CQELight.IoC.Autofac;
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
            new Bootstrapper()
                .ConfigureDispatcher(GetCoreDispatcherConfiguration())
                .UseAutofacAsIoC(c =>
                {
                });

            Console.WriteLine("Message manager");
            Console.WriteLine("Enter '/q' to exit system");

            string message = string.Empty;
            while(true)
            {
                Console.WriteLine("Enter your message to be proceed by system : ");
                message = Console.ReadLine();
                if (message == "/q")
                {
                    break;
                }
                else
                {
                    await CoreDispatcher.DispatchCommandAsync(new SendMessageCommand(message));
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
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("An error has been raised during dispatch : " + e);
                    Console.ForegroundColor = ConsoleColor.White;
                })
                .SerializeWith<JsonEventSerializer>();
            return configurationBuilder.Build();
        }
    }
}
