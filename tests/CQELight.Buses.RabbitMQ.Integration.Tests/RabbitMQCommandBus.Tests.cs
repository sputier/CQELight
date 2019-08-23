using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Buses.RabbitMQ.Client;
using CQELight.Events.Serializers;
using CQELight.TestFramework;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using CQELight.Tools.Extensions;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Buses.RabbitMQ.Integration.Tests
{
    public class RabbitMQCommandBusTests : BaseUnitTestClass
    {
        #region Ctor & members
        private class RabbitCommand : ICommand
        {
            public string Data { get; set; }
        }
        
        private IModel _channel;
        private readonly IConfiguration _testConfiguration;

        public RabbitMQCommandBusTests()
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

        #region CommandBus

        [Fact]
        public async Task DispatchCommandAsync_Should_AddItToExchange()
        {
            var appId = Guid.NewGuid();
            var queueName = "test_queue_commands_1";
            try
            {
                _channel.ExchangeDeclare(appId + "_commands", ExchangeType.Topic, true, false);
                _channel.QueueDeclare(queueName, true, true, false);
                _channel.QueueBind(queueName, appId + "_commands", "CQELight");

                var evt = new RabbitCommand
                {
                    Data = "testData"
                };

                var b = new RabbitMQCommandBus(
                    new JsonDispatcherSerializer(),
                    new RabbitMQClientBusConfiguration(appId.ToString(), _testConfiguration["host"], _testConfiguration["user"], _testConfiguration["password"]));
                var allCalled = false;

                await b.DispatchAsync(evt).ContinueWith(t =>
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
                    evet.Should().BeOfType<RabbitCommand>();
                    evet.As<RabbitCommand>().Data.Should().Be("testData");
                    allCalled = true;
                }).ConfigureAwait(false);

                allCalled.Should().BeTrue();
            }
            finally
            {
                _channel.QueueDelete(queueName);
                _channel.ExchangeDelete(appId + "_commands");
            }
        }

        #endregion

    }
}
