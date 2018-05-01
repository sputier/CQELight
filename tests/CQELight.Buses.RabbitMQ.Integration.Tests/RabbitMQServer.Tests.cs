using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.RabbitMQ.Client;
using CQELight.Buses.RabbitMQ.Server;
using CQELight.Events.Serializers;
using CQELight.TestFramework;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
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

        private class RabbitCommand : ICommand
        {
            public string Data { get; set; }
        }

        private Mock<ILoggerFactory> _loggerFactory;
        private IConfiguration _configuration;
        private RabbitMQClientBus _client;

        public RabbitMQServerTests()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("test-config.json").Build();
            _loggerFactory = new Mock<ILoggerFactory>();
            _client = new RabbitMQClientBus(new JsonDispatcherSerializer(), new RabbitMQClientBusConfiguration(_configuration["host"], _configuration["user"],
                _configuration["password"]));
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

            channel.ExchangeDelete(exchange: Consts.CONSTS_CQE_EXCHANGE_NAME);
            channel.QueueDelete(Consts.CONST_QUEUE_NAME_EVENTS);
            channel.QueueDelete(Consts.CONST_QUEUE_NAME_COMMANDS);

        }
    
        #endregion

        #region Start

        [Fact]
        public async Task RabbitMQServer_Start_Callback_AsExpected()
        {
            bool evtCalled = false;
            bool cmdCalled = false;
            IDomainEvent evt = null;
            ICommand cmd = null;

            var evtToSend = new RabbitEvent { Data = "evt_data" };
            var cmdToSend = new RabbitCommand { Data = "cmd_data" };

            var server = new RabbitMQServer(_loggerFactory.Object, new RabbitMQServerConfiguration(_configuration["host"], _configuration["user"],
                _configuration["password"],
                new EventQueueConfiguration(new JsonDispatcherSerializer(), false,
                o =>
                {
                    if (o is IDomainEvent receivedEvt)
                    {
                        evtCalled = true;
                        evt = receivedEvt;
                    }
                }),
                new CommandQueueConfiguration(new JsonDispatcherSerializer(), false,
                o =>
                {
                    if (o is ICommand receveidCmd)
                    {
                        cmdCalled = true;
                        cmd = receveidCmd;
                    }
                })));

            server.Start();

            await _client.RegisterAsync(evtToSend).ConfigureAwait(false);
            await _client.DispatchAsync(cmdToSend).ConfigureAwait(false);

            evtCalled.Should().BeTrue();
            cmdCalled.Should().BeTrue();

            evt.Should().NotBeNull();
            cmd.Should().NotBeNull();

            evt.Should().BeOfType<RabbitEvent>();
            evt.As<RabbitEvent>().Data.Should().Be("evt_data");

            cmd.Should().BeOfType<RabbitCommand>();
            cmd.As<RabbitCommand>().Data.Should().Be("cmd_data");
        }

        [Fact]
        public async Task RabbitMQServer_Start_InMemory_AsExpected()
        {

        }

        #endregion

    }
}
