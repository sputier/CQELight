using CQELight.Abstractions.Events;
using CQELight.Buses.RabbitMQ.Client;
using CQELight.Events.Serializers;
using CQELight.TestFramework;
using FluentAssertions;
using RabbitMQ.Client;
using CQELight.Tools.Extensions;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using CQELight.Abstractions.CQS.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CQELight.Buses.RabbitMQ.Integration.Tests
{
    public class RabbitMQEventBusTests : BaseUnitTestClass
    {
        #region Ctor & members

        private class RabbitEvent : BaseDomainEvent
        {
            public string Data { get; set; }
        }

        private IModel _channel;
        private readonly IConfiguration _testConfiguration;

        public RabbitMQEventBusTests()
        {
            _testConfiguration = new ConfigurationBuilder().AddJsonFile("test-config.json").Build();
            CreateChannel();
        }

        private void CreateChannel()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _testConfiguration["host"],
                UserName = _testConfiguration["user"],
                Password = _testConfiguration["password"]
            };
            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();
        }

        #endregion

        #region PublishEventAsync

        [Fact]
        public async Task RabbitMQClientBus_PublishEventAsync_Should_Be_Readable()
        {
            var appId = Guid.NewGuid();
            const string queueName = "test_publish_queue_1";
            try
            {
                _channel.ExchangeDeclare(
                    exchange: appId + "_events",
                    type: ExchangeType.Fanout, 
                    durable : true, 
                    autoDelete: false);
                _channel.QueueDeclare(queueName, false, true, true);
                _channel.QueueBind(queueName, appId + "_events", "");

                var evt = new RabbitEvent
                {
                    Data = "testData"
                };

                var b = new RabbitMQEventBus(
                    new JsonDispatcherSerializer(),
                    new RabbitMQClientBusConfiguration(appId.ToString(), _testConfiguration["host"], _testConfiguration["user"], _testConfiguration["password"]));
                var allCalled = false;

                await b.PublishEventAsync(evt).ContinueWith(t =>
                {
                    t.Result.IsSuccess.Should().BeTrue();
                    var result = _channel.BasicGet(queueName, true);
                    result.Should().NotBeNull();
                    var enveloppeAsStr = Encoding.UTF8.GetString(result.Body);
                    enveloppeAsStr.Should().NotBeNullOrWhiteSpace();

                    var receivedEnveloppe = enveloppeAsStr.FromJson<Enveloppe>();
                    receivedEnveloppe.Should().NotBeNull();

                    var type = Type.GetType(receivedEnveloppe.AssemblyQualifiedDataType);
                    var evet = receivedEnveloppe.Data.FromJson(type);
                    evet.Should().BeOfType<RabbitEvent>();
                    evet.As<RabbitEvent>().Data.Should().Be("testData");
                    allCalled = true;
                }).ConfigureAwait(false);

                allCalled.Should().BeTrue();
            }
            finally
            {
                _channel.ExchangeDelete(appId + "_events");
                _channel.QueueDelete(queueName);
            }
        }

        #endregion

    }
}
