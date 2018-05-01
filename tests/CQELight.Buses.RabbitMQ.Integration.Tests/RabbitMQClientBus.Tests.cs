using CQELight.Abstractions.Events;
using CQELight.Buses.RabbitMQ.Client;
using CQELight.Events.Serializers;
using CQELight.TestFramework;
using FluentAssertions;
using RabbitMQ.Client;
using CQELight.Tools.Extensions;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using CQELight.Abstractions.CQS.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CQELight.Buses.RabbitMQ.Integration.Tests
{
    public class RabbitMQClientBusTests : BaseUnitTestClass
    {

        #region Ctor & members

        private class RabbitEvent : BaseDomainEvent
        {
            public string Data { get; set; }
        }

        private class RabbitCommand : ICommand
        {
            public string Data { get; set; }
        }

        public IModel _channel;
        public IConfiguration _testConfiguration;

        public RabbitMQClientBusTests()
        {
            _testConfiguration = new ConfigurationBuilder().AddJsonFile("test-config.json").Build();
            if (_testConfiguration["host"] == "localhost" && !Directory.Exists(@"C:\Program Files\RabbitMQ Server"))
            {
                Assert.False(true, "It seems RabbitMQ is not installed on your system.");
            }
        }

        private void CreateQueue()
        {
            var factory = new ConnectionFactory() { HostName = RabbitMQClientBusConfiguration.Default.Host };
            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();

            _channel.ExchangeDelete(exchange: Consts.CONSTS_CQE_EXCHANGE_NAME);
            _channel.ExchangeDeclare(exchange: Consts.CONSTS_CQE_EXCHANGE_NAME,
                        type: ExchangeType.Fanout,
                        durable: true,
                        autoDelete: false);

            _channel.QueueDelete(Consts.CONST_QUEUE_NAME_EVENTS);
            _channel.QueueDeclare(
                            queue: Consts.CONST_QUEUE_NAME_EVENTS,
                            durable: true,
                            exclusive: false,
                            autoDelete: false);
            _channel.QueueBind(Consts.CONST_QUEUE_NAME_EVENTS, Consts.CONSTS_CQE_EXCHANGE_NAME, Consts.CONST_EVENTS_ROUTING_KEY);

            _channel.QueueDelete(Consts.CONST_QUEUE_NAME_COMMANDS);
            _channel.QueueDeclare(
                            queue: Consts.CONST_QUEUE_NAME_COMMANDS,
                            durable: true,
                            exclusive: false,
                            autoDelete: false);
            _channel.QueueBind(Consts.CONST_QUEUE_NAME_COMMANDS, Consts.CONSTS_CQE_EXCHANGE_NAME, Consts.CONST_COMMANDS_ROUTING_KEY);
        }

        #endregion

        #region RegisterAsync

        [Fact]
        public async Task RabbitMQClientBus_RegisterAsync_AsExpected()
        {
            var evt = new RabbitEvent
            {
                Data = "testData"
            };

            var b = new RabbitMQClientBus(
                new JsonDispatcherSerializer(),
                RabbitMQClientBusConfiguration.Default);

            CreateQueue();

            await b.RegisterAsync(evt).ContinueWith(t =>
            {
                var result = _channel.BasicGet(Consts.CONST_QUEUE_NAME_EVENTS, true);
                result.Should().NotBeNull();
                var data = Encoding.UTF8.GetString(result.Body);
                data.Should().NotBeNullOrWhiteSpace();

                var receivedEvt = data.FromJson<RabbitEvent>();
                receivedEvt.Should().NotBeNull();
                receivedEvt.Data.Should().Be("testData");
            });
        }

        #endregion

        #region DispatchAsync

        [Fact]
        public async Task RabbitMQClientBus_DispatchAsync_AsExpected()
        {
            var cmd = new RabbitCommand
            {
                Data = "testData"
            };

            var b = new RabbitMQClientBus(
                new JsonDispatcherSerializer(),
                RabbitMQClientBusConfiguration.Default);

            CreateQueue();

            await b.DispatchAsync(cmd).ContinueWith(t =>
            {
                var result = _channel.BasicGet(Consts.CONST_QUEUE_NAME_COMMANDS, true);
                result.Should().NotBeNull();
                var data = Encoding.UTF8.GetString(result.Body);
                data.Should().NotBeNullOrWhiteSpace();

                var receivedCmd = data.FromJson<RabbitCommand>();
                receivedCmd.Should().NotBeNull();
                receivedCmd.Data.Should().Be("testData");
            });
        }

        #endregion

    }
}
