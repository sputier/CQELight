using Autofac;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Buses.RabbitMQ.Client;
using CQELight.Buses.RabbitMQ.Server;
using CQELight.Dispatcher;
using CQELight.Events.Serializers;
using CQELight.IoC;
using CQELight.TestFramework;
using CQELight.Tools.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using System;
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

        private static readonly SemaphoreSlim sem = new SemaphoreSlim(1);
        private readonly Mock<ILoggerFactory> _loggerFactory;
        private readonly IConfiguration _configuration;
        private IModel _channel;

        private const string CONST_APP_ID = "BA3F9093-D7EE-4BB8-9B4E-EEC3447A89BA";
        private readonly string _queueName = "cqelight.events" + CONST_APP_ID;

        public RabbitMQServerTests()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("test-config.json").Build();
            _loggerFactory = new Mock<ILoggerFactory>();
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

            _channel = connection.CreateModel();

            _channel.ExchangeDelete(exchange: Consts.CONST_CQE_EXCHANGE_NAME);
            _channel.QueueDelete(queue: _queueName);
        }

        #endregion

        #region Start

        [Fact]
        public async Task RabbitMQServer_SameApp_Start_Callback_AsExpected()
        {
            //bool finished = false;
            //var evtToSend = new RabbitEvent { Data = "evt_data" };

            //var server = new RabbitMQServer(_loggerFactory.Object,
            //    new RabbitMQServerConfiguration(CONST_APP_ID, _configuration["host"], _configuration["user"], _configuration["password"],
            //    new QueueConfiguration(new JsonDispatcherSerializer(), _queueName, false,
            //    o =>
            //    {
            //        if (o is IDomainEvent receivedEvt)
            //        {
            //            receivedEvt.Should().BeOfType<RabbitEvent>();
            //            receivedEvt.As<RabbitEvent>().Data.Should().Be("evt_data");
            //        }
            //        finished = true;
            //    })));

            //server.Start();

            //var factory = new ConnectionFactory
            //{
            //    HostName = _configuration["host"],
            //    UserName = _configuration["user"],
            //    Password = _configuration["password"]
            //};
            //using (var connection = factory.CreateConnection())
            //{
            //    using (var channel = connection.CreateModel())
            //    {
            //        channel.BasicPublish(
            //            exchange: Consts.CONST_CQE_EXCHANGE_NAME,//CONST_APP_ID + "_events",
            //            routingKey: "",
            //            body: Encoding.UTF8.GetBytes(new Enveloppe(evtToSend.ToJson(), typeof(RabbitEvent), "me").ToJson()));
            //    }
            //}

            //long spentTime = 0;
            //while (!finished && spentTime < 2000)
            //{
            //    spentTime += 50;
            //    await Task.Delay(50).ConfigureAwait(false);
            //}
            //finished.Should().BeTrue();
        }

        private class RabbitHandler : IDomainEventHandler<RabbitEvent>, IAutoRegisterType
        {
            public static event Action<RabbitEvent> OnEventArrived;
            public Task<Result> HandleAsync(RabbitEvent domainEvent, IEventContext context = null)
            {
                OnEventArrived?.Invoke(domainEvent);
                return Result.Ok();
            }
        }

        [Fact]
        public async Task RabbitMQServer_SameApp_Start_InMemory_AsExpected()
        {
            //try
            //{
            //    var evtToSend = new RabbitEvent { Data = "evt_data" };

            //    new Bootstrapper()
            //        .UseInMemoryEventBus()
            //        .UseRabbitMQServer()
            //        .Bootstrapp();

            //    var server = new RabbitMQServer(null, new RabbitMQServerConfiguration(CONST_APP_ID, _configuration["host"], _configuration["user"],
            //            _configuration["password"], new QueueConfiguration(new JsonDispatcherSerializer(), "cqelight.events", true, null)));

            //    server.Start();

            //    var factory = new ConnectionFactory
            //    {
            //        HostName = _configuration["host"],
            //        UserName = _configuration["user"],
            //        Password = _configuration["password"]
            //    };
            //    using (var connection = factory.CreateConnection())
            //    {
            //        using (var channel = connection.CreateModel())
            //        {
            //            channel.BasicPublish(
            //                exchange: Consts.CONST_CQE_EXCHANGE_NAME,//CONST_APP_ID + "_events",
            //                routingKey: Consts.CONST_ROUTING_KEY_ALL,
            //                body: Encoding.UTF8.GetBytes(new Enveloppe(evtToSend.ToJson(), typeof(RabbitEvent), "me").ToJson()));
            //        }
            //    }

            //    string receivedData = string.Empty;
            //    bool finished = false;
            //    RabbitHandler.OnEventArrived += (e) =>
            //    {
            //        if (e is RabbitEvent rabbitEv)
            //        {
            //            receivedData = rabbitEv.Data;
            //            finished = true;
            //        }
            //    };

            //    long spentTime = 0;

            //    while (!finished && spentTime < 2000)
            //    {
            //        spentTime += 50;
            //        await Task.Delay(50).ConfigureAwait(false);
            //    }
            //    receivedData.Should().Be("evt_data");

            //}
            //finally
            //{
            //    sem.Release();
            //}
        }
    }

    #endregion

}

