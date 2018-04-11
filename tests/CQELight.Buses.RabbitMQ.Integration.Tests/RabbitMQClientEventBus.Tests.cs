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

namespace CQELight.Buses.RabbitMQ.Integration.Tests.cs
{
    public class RabbitMQClientBusBusTests : BaseUnitTestClass
    {

        #region Ctor & members

        private class RabbitEvent : BaseDomainEvent
        {
            public string Data { get; set; }
        }

        public IModel _eventChannel;

        public RabbitMQClientBusBusTests()
        {
            if (!Directory.Exists(@"C:\Program Files\RabbitMQ Server"))
            {
                Assert.False(true, "It seems RabbitMQ is not installed on your system.");
            }

            var factory = new ConnectionFactory() { HostName = RabbitMQClientBusConfiguration.Default.Host };
            var connection = factory.CreateConnection();

            _eventChannel = connection.CreateModel();
            _eventChannel.QueueDelete("cqe_event_queue");
            _eventChannel.QueueDeclare(
                            queue: "cqe_event_queue",
                            durable: true,
                            exclusive: false,
                            autoDelete: false);
            _eventChannel.QueueBind("cqe_event_queue", Consts.CONST_EVENTS_EXCHANGE_NAME, Consts.CONST_EVENTS_ROUTING_KEY);
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
            await b.RegisterAsync(evt).ConfigureAwait(false);

            var result = _eventChannel.BasicGet("cqe_event_queue", true);
            result.Should().NotBeNull();
            var data = Encoding.UTF8.GetString(result.Body);
            data.Should().NotBeNullOrWhiteSpace();

            var receivedEvt = data.FromJson<RabbitEvent>();
            receivedEvt.Should().NotBeNull();
            receivedEvt.Data.Should().Be("testData");
        }

        #endregion

    }
}
