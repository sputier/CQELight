using Autofac;
using CQELight.Dispatcher;
using CQELight.Dispatcher.Configuration;
using CQELight.Events.Serializers;
using CQELight.Examples.ConsoleApp.Commands;
using CQELight.Examples.ConsoleApp.Events;
using CQELight.Implementations.Events.InMemory.Stateless;
using CQELight.Implementations.Events.System;
using CQELight.Implementations.IoC.Autofac;
using CQELight.IoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Examples.ConsoleApp
{
    class Program
    {
        internal static string FriendlyName;
        internal static Guid Id;

        static void Main(string[] args)
        {
            #region Program configuration

            Console.WriteLine(" -------------- Chat with System bus -------------- ");
            //Configuration IoC
            var containerBuilder = new ContainerBuilder();
            //TODO
            DIManager.Init(new AutofacScopeFactory(containerBuilder.Build()));

            //Configuration Dispatcher
            var builder = new CoreDispatcherConfigurationBuilder();

            builder.ForAllEvents()
                .UseBuses(typeof(SystemEventBus), typeof(InMemoryStatelessEventBus))
                .HandleErrorWith(e => Console.WriteLine($"ERROR : {e}"))
                .SerializeWith<JsonEventSerializer>();

            CoreDispatcher.UseConfiguration(builder.Build());

            var dotnetProcess = Process.GetProcessesByName("dotnet");
            //I launche bus if it isn't launched
            if (!dotnetProcess.Select(p => GetCommandLines(p)).Any(c => c.Contains("CQELight.SystemBus")))
            {
                Console.WriteLine("Launching bus");
                //TODO
                //Process.Start(new ProcessStartInfo("dotnet", $@"exec """""));
            }
            #endregion

            Id = Guid.NewGuid();
            Console.WriteLine($"Id of this session : {Id}");
            Console.WriteLine("Name on the chat : ");
            FriendlyName = Console.ReadLine();
            //Bus config => give the event 30s lifetime
            var typeLifetime = new System.Collections.Concurrent.ConcurrentDictionary<Type, ulong>();
            typeLifetime.AddOrUpdate(typeof(MessageSentEvent), 30000, (t, u) => u);
            typeLifetime.AddOrUpdate(typeof(ClientConnectedEvent), 30000, (t, u) => u);
            CoreDispatcher.ConfigureBus<SystemEventBus, SystemEventBusConfiguration>(new SystemEventBusConfiguration(Id, FriendlyName)
            {
                TypeLifetime = typeLifetime
            });
            //When I'm connected, I dispatch the event to the system
            CoreDispatcher.DispatchEvent(new ClientConnectedEvent { FriendlyName = FriendlyName, ClientID = Id });
            Console.WriteLine(@"Write messages. \q to quit chat.");
            var message = Console.ReadLine();
            while (message != @"\q")
            {
                //I want to send a message, it's a command
                CoreDispatcher.DispatchCommand(new SendMessageCommand(message));
                message = Console.ReadLine();
            }
        }

        private static string GetCommandLines(Process processs)
        {
            var commandLineSearcher = new ManagementObjectSearcher(
                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processs.Id}");
            string commandLine = "";
            foreach (ManagementObject commandLineObject in commandLineSearcher.Get())
            {
                commandLine += commandLineObject["CommandLine"].ToString();
            }

            return commandLine;
        }
    }
}
