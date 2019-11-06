using CQELight.Abstractions.Events;
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
using CQELight.Buses.RabbitMQ.Configuration.Publisher;
using CQELight.Buses.RabbitMQ.Publisher;

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
        const string queueName = "cqelight_rabbit_test_publish_queue_1";
        const string specificQueueName = "cqelight_rabbit_test_specific_queue_1";
        const string specificExchangeName = "CQELight_Specific_Exchange";
        const string defaultExchangeName = "CQELight_Test_events";

        public RabbitMQEventBusTests()
        {
            _testConfiguration = new ConfigurationBuilder().AddJsonFile("test-config.json").Build();
            CreateChannel();
            DeleteData();
        }

        private void DeleteData()
        {
            _channel.ExchangeDelete(defaultExchangeName);
            _channel.ExchangeDelete(specificExchangeName);
            _channel.QueueDelete(queueName);
            _channel.QueueDelete(specificQueueName);
        }

        private ConnectionFactory GetConnectionFactory() =>
            new ConnectionFactory()
            {
                HostName = _testConfiguration["host"],
                UserName = _testConfiguration["user"],
                Password = _testConfiguration["password"]
            };

        private void CreateChannel()
        {
            var factory = GetConnectionFactory();
            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();
        }

        private void DeclareStandardExchangeAndQueue()
        {
            _channel.ExchangeDeclare(
                                exchange: "CQELight_Test_events",
                                type: ExchangeType.Fanout,
                                durable: true,
                                autoDelete: false);
            _channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false);
            _channel.QueueBind(queueName, "CQELight_Test_events", "");
        }

        #endregion

        #region PublishEventAsync

        [Fact]
        public async Task RabbitMQEventBus_PublishEventAsync_DefaultConfig_Should_Publish_To_Exchange()
        {
            try
            {
                DeclareStandardExchangeAndQueue();

                var evt = new RabbitEvent
                {
                    Data = "testData"
                };

                var b = new RabbitMQEventBus(
                    new JsonDispatcherSerializer(),
                    new RabbitPublisherBusConfiguration("CQELight_Test", GetConnectionFactory()));

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
                DeleteData();
            }
        }
        
        [Fact]
        public async Task RabbitMQEventBus_PublishEventAsync_Should_RespectConfiguration()
        {
            try
            {
                var configBuilder = new RabbitPublisherConfigurationBuilder();
                configBuilder.ForEvent<RabbitEvent>().UseExchange(specificExchangeName);

                DeclareStandardExchangeAndQueue();
                _channel.ExchangeDeclare(
                    exchange: specificExchangeName,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false);
                _channel.QueueDeclare(specificQueueName, durable: false, exclusive: false, autoDelete: false);
                _channel.QueueBind(specificQueueName, specificExchangeName, "");

                var evt = new RabbitEvent
                {
                    Data = "testData"
                };

                var b = new RabbitMQEventBus(
                    new JsonDispatcherSerializer(),
                    new RabbitPublisherBusConfiguration("CQELight_Test", GetConnectionFactory(), configBuilder.GetConfiguration()));

                var allCalled = false;

                var result = await b.PublishEventAsync(evt);
                result.IsSuccess.Should().BeTrue();

                _channel.BasicGet(queueName, true).Should().BeNull();

                var data = _channel.BasicGet(specificQueueName, true);
                data.Should().NotBeNull();
                var enveloppeAsStr = Encoding.UTF8.GetString(data.Body);
                enveloppeAsStr.Should().NotBeNullOrWhiteSpace();

                var receivedEnveloppe = enveloppeAsStr.FromJson<Enveloppe>();
                receivedEnveloppe.Should().NotBeNull();

                var type = Type.GetType(receivedEnveloppe.AssemblyQualifiedDataType);
                var evet = receivedEnveloppe.Data.FromJson(type);
                evet.Should().BeOfType<RabbitEvent>();
                evet.As<RabbitEvent>().Data.Should().Be("testData");
                allCalled = true;

                allCalled.Should().BeTrue();
            }
            finally
            {
                DeleteData();
            }

        }

        #endregion

    }
}
