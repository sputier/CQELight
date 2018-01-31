using Autofac;
using CQELight.Buses.InMemory.Commands;
using CQELight.Buses.InMemory.Events;
using CQELight.Buses.RabbitMQ.Events;
using CQELight.Buses.RabbitMQ.Server;
using CQELight.Dispatcher;
using CQELight.Dispatcher.Configuration;
using CQELight.Events.Serializers;
using CQELight.Examples.ConsoleApp.Commands;
using CQELight.Examples.ConsoleApp.Events;
using CQELight.IoC;
using CQELight.IoC.Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace CQELight.Examples.ConsoleApp
{
    class Program
    {
        internal static string FriendlyName;
        internal static Guid Id;

        static async Task Main(string[] args)
        {
            #region Program configuration

            Console.WriteLine(" -------------- Chat with RabbitMQ bus -------------- ");
            Id = Guid.NewGuid();

            ConfigureIoCContainer();
            ConfigreDispatcher();
            ConfigureRabbitMQ();

            #endregion
            //start RabbitMQ local server
            using (var scope = DIManager.BeginScope())
            {
                using (var rmServer = new RabbitMQServer(
                    async evt =>
                    {
                        await scope.Resolve<InMemoryEventBus>().RegisterAsync(evt);
                    },
                    scope.Resolve<ILoggerFactory>(),
                    new RabbitMQServerConfiguration("localhost", Id.ToString())))
                {
                    rmServer.Start();
                    Console.WriteLine($"Id of this session : {Id}");
                    Console.WriteLine("Name on the chat : ");
                    FriendlyName = Console.ReadLine();
                    //When I'm connected, I dispatch the event to the system
                    await CoreDispatcher.DispatchEventAsync(new ClientConnectedEvent { FriendlyName = FriendlyName, ClientID = Id });
                    Console.WriteLine(@"Write messages. \q to quit chat.");
                    var message = Console.ReadLine();
                    while (message != @"\q")
                    {
                        //I want to send a message, it's a command
                        await CoreDispatcher.DispatchCommandAsync(new SendMessageCommand(message));
                        message = Console.ReadLine();
                    }
                }
            }
        }

        private static void ConfigureRabbitMQ()
        {
            var rabbitMQPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData\\Roaming\\RabbitMQ");
            if (!Directory.Exists(rabbitMQPath))
            {
                Console.WriteLine("It seems like RabbitMQ is not installed on your system. Please install it first. Press any key to close app.");
                Console.Read();
                Environment.Exit(0);
            }

            var rabbitMQVersion = ConfigurationManager.AppSettings["rabbitMQVersion"];
            var rabbitMQService = $"C:\\Program Files\\RabbitMQ Server\\rabbitmq_server-{rabbitMQVersion}\\sbin\\rabbitmq-service.bat";
            Process.Start(new ProcessStartInfo
            {
                FileName = rabbitMQService,
                Arguments = "start"
            });
        }

        private static void ConfigreDispatcher()
        {
            var builder = new CoreDispatcherConfigurationBuilder();
            Action<Exception> errorLambda = (Exception e) => Console.WriteLine($"ERROR : {e}");
            builder.ForEvent<ClientConnectedEvent>()
                .UseBuses(typeof(RabbitMQClientEventBus), typeof(InMemoryEventBus))
                .HandleErrorWith(errorLambda)
                .SerializeWith<JsonEventSerializer>();
            builder.ForEvent<MessageSentEvent>()
                .UseBuses(typeof(RabbitMQClientEventBus), typeof(InMemoryEventBus))
                .HandleErrorWith(errorLambda)
                .SerializeWith<JsonEventSerializer>();

            CoreDispatcher.UseConfiguration(builder.Build());

            CoreDispatcher.ConfigureBus<RabbitMQClientEventBus, RabbitMQClientEventBusConfiguration>(
                new RabbitMQClientEventBusConfiguration("localhost"));
            CoreDispatcher.ConfigureBus<InMemoryEventBus, InMemoryEventBusConfiguration>(
                new InMemoryEventBusConfiguration(3, 250, (e, c) => { }));
        }

        private static void ConfigureIoCContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<AutoRegisterModule>();
            containerBuilder.Register(c =>
            {
                var loggerFactory = new LoggerFactory();
                loggerFactory.AddConsole((LogLevel)Enum.Parse(typeof(LogLevel), ConfigurationManager.AppSettings["minLogLevel"]));
                return loggerFactory;
            }).AsImplementedInterfaces().AsSelf();
            var fullCtorFinder = new FullConstructorFinder();
            containerBuilder.RegisterType<InMemoryEventBus>()
                .AsSelf()
                .AsImplementedInterfaces()
                .FindConstructorsWith(fullCtorFinder);
            containerBuilder.RegisterType<InMemoryCommandBus>()
                .AsSelf()
                .AsImplementedInterfaces()
                .FindConstructorsWith(fullCtorFinder);
            containerBuilder.RegisterType<RabbitMQClientEventBus>()
                .AsSelf()
                .AsImplementedInterfaces()
                .FindConstructorsWith(fullCtorFinder);
            DIManager.Init(new AutofacScopeFactory(containerBuilder.Build()));
        }
    }
}
