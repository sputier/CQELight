using Autofac;
using CQELight.Abstractions.Configuration;
using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Bootstrapping.Notifications;
using CQELight.Buses.InMemory;
using CQELight.Buses.RabbitMQ.Client;
using CQELight.Buses.RabbitMQ.Extensions;
using CQELight.Buses.RabbitMQ.Server;
using CQELight.Configuration;
using CQELight.Events.Serializers;
using CQELight.IoC;
using CQELight.IoC.Autofac;
using CQELight.TestFramework;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Buses.RabbitMQ.Integration.Tests
{
    public class RabbitMQServerTests : BaseUnitTestClass
    {
        #region Ctor & members

        private class RabbitEvent : BaseDomainEvent
        {
            public string Data { get; set; }
        }

        private class RabbitEventHandler : IDomainEventHandler<RabbitEvent>, IAutoRegisterType
        {
            public static string ReceivedData { get; set; }

            public Task HandleAsync(RabbitEvent domainEvent, IEventContext context = null)
            {
                ReceivedData = domainEvent.Data;
                sem.Release();
                return Task.CompletedTask;
            }
        }

        private static readonly SemaphoreSlim sem = new SemaphoreSlim(1);
        private readonly Mock<ILoggerFactory> _loggerFactory;
        private readonly IConfiguration _configuration;
        private readonly RabbitMQClientBus _client;
        private const string CONST_APP_ID_SERVER = "BA3F9093-D7EE-4BB8-9B4E-EEC3447A89BA";
        private AppId _appIdServer;
        private const string CONST_APP_ID_CLIENT = "AA3F9093-D7EE-4BB8-9B4E-EEC3447A89BA";
        private AppId _appIdClient;
        private readonly Mock<IAppIdRetriever> _appIdClientRetrieverMock;
        private readonly Mock<IAppIdRetriever> _appIdServerRetrieverMock;
        private readonly string _queueName = Consts.CONST_QUEUE_NAME_PREFIX + CONST_APP_ID_SERVER.ToLower();

        public RabbitMQServerTests()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("test-config.json").Build();
            _loggerFactory = new Mock<ILoggerFactory>();
            _appIdServer = new AppId(Guid.Parse(CONST_APP_ID_SERVER));
            _appIdClient = new AppId(Guid.Parse(CONST_APP_ID_CLIENT));
            _appIdClientRetrieverMock = new Mock<IAppIdRetriever>();
            _appIdClientRetrieverMock.Setup(m => m.GetAppId()).Returns(_appIdClient);
            _appIdServerRetrieverMock = new Mock<IAppIdRetriever>();
            _appIdServerRetrieverMock.Setup(m => m.GetAppId()).Returns(_appIdServer);
            _client = new RabbitMQClientBus(_appIdClientRetrieverMock.Object, new JsonDispatcherSerializer(),
                new RabbitMQClientBusConfiguration(_configuration["host"], _configuration["user"], _configuration["password"]));
            CleanQueues();
        }

        private void CleanQueues()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["host"],
                UserName = _configuration["user"],
                Password = _configuration["password"]
            };
            var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.ExchangeDelete(exchange: Consts.CONST_CQE_EXCHANGE_NAME);
            channel.QueueDelete(queue: _queueName);
        }

        #endregion

        #region Start

        [Fact]
        public async Task RabbitMQServer_Start_Callback_AsExpected()
        {
            bool finished = false;
            var evtToSend = new RabbitEvent { Data = "evt_data" };

            var server = new RabbitMQServer(_appIdServerRetrieverMock.Object, _loggerFactory.Object,
                new RabbitMQServerConfiguration(_configuration["host"], _configuration["user"], _configuration["password"],
                new QueueConfiguration(new JsonDispatcherSerializer(), "", false,
                o =>
                {
                    if (o is IDomainEvent receivedEvt)
                    {
                        receivedEvt.Should().BeOfType<RabbitEvent>();
                        receivedEvt.As<RabbitEvent>().Data.Should().Be("evt_data");
                    }
                    finished = true;
                })));

            server.Start();

            await _client.PublishEventAsync(evtToSend).ConfigureAwait(false);
            while (!finished)
            {
                await Task.Delay(50).ConfigureAwait(false);
            }
            finished.Should().BeTrue();
        }

        [Fact]
        public async Task RabbitMQServer_Start_InMemory_AsExpected()
        {
            try
            {
                var evtToSend = new RabbitEvent { Data = "evt_data" };

                var cb = new Autofac.ContainerBuilder();

                cb.Register(c => new LoggerFactory()).AsImplementedInterfaces();
                cb.Register(c => _appIdServerRetrieverMock.Object).AsImplementedInterfaces();

                new Bootstrapper()
                    .UseInMemoryEventBus()
                    .UseAutofacAsIoC(cb)
                    .UseRabbitMQServer(
                        new RabbitMQServerConfiguration(_configuration["host"], _configuration["user"],
                        _configuration["password"], new QueueConfiguration(new JsonDispatcherSerializer(), "", true, null))
                    )
                    .Bootstrapp();

                using (var scope = DIManager.BeginScope())
                {
                    await sem.WaitAsync().ConfigureAwait(false);
                    var server = scope.Resolve<RabbitMQServer>();

                    server.Start();

                    await _client.PublishEventAsync(evtToSend).ConfigureAwait(false);

                    await sem.WaitAsync().ConfigureAwait(false);

                    RabbitEventHandler.ReceivedData.Should().Be("evt_data");
                }
            }
            finally
            {
                sem.Release();
            }
        }
    }

    #endregion

}

