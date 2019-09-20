using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Buses.RabbitMQ.Subscriber;
using CQELight.Buses.RabbitMQ.Subscriber.Configuration;
using CQELight.Events.Serializers;
using CQELight.TestFramework;
using CQELight.Tools.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Buses.RabbitMQ.Integration.Tests
{
    public class RabbitMQSubscriberTests : BaseUnitTestClass
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

        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
        private IModel _channel;

        const string subscriberName = "subscriber_test";

        const string publisher1Name = "prod1";
        const string publisher2Name = "prod2";

        const string firstProducerEventExchangeName = publisher1Name + "_events";
        const string firstProducerCommandExchangeName = publisher1Name + "_commands";
        const string secondProducerEventExchangeName = publisher2Name + "_events";
        const string secondProducerCommandExchangeName = publisher2Name + "_commands";
        private const string QueueName = "subscriber_test_queue";
        private const string QueueNameAckStrategy = "subscriber_test_queue_ack_strategy";

        public RabbitMQSubscriberTests()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("test-config.json").Build();
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new DebugLoggerProvider());
            CleanQueues();
            DeleteData();
        }

        private void DeleteData()
        {
            try
            {
                _channel.ExchangeDelete(firstProducerEventExchangeName);
                _channel.ExchangeDelete(firstProducerCommandExchangeName);
                _channel.ExchangeDelete(secondProducerEventExchangeName);
                _channel.ExchangeDelete(secondProducerCommandExchangeName);
                _channel.ExchangeDelete(Consts.CONST_DEAD_LETTER_EXCHANGE_NAME);
                _channel.QueueDelete(QueueName);
                _channel.QueueDelete(QueueNameAckStrategy);
                _channel.QueueDelete(Consts.CONST_DEAD_LETTER_QUEUE_NAME);
            }
            catch { }
        }

        private ConnectionFactory GetConnectionFactory()
            => new ConnectionFactory()
            {
                HostName = _configuration["host"],
                UserName = _configuration["user"],
                Password = _configuration["password"]
            };

        private void CleanQueues()
        {
            var factory = GetConnectionFactory();
            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();
        }

        private void CreateExchanges()
        {
            _channel.ExchangeDeclare(firstProducerEventExchangeName, "fanout", true, false);
            _channel.ExchangeDeclare(firstProducerCommandExchangeName, "topic", true, false);
            _channel.ExchangeDeclare(secondProducerEventExchangeName, "fanout", true, false);
            _channel.ExchangeDeclare(secondProducerCommandExchangeName, "topic", true, false);
        }

        private byte[] GetEnveloppeDataForEvent(string publisher, string content)
        {
            var evt = new RabbitEvent { Data = content };
            var ev = new Enveloppe(JsonConvert.SerializeObject(evt), typeof(RabbitEvent), publisher);
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ev));
        }

        private byte[] GetEnveloppeDataForCommand(string publisher, string content)
        {
            var evt = new RabbitCommand { Data = content };
            var ev = new Enveloppe(JsonConvert.SerializeObject(evt), typeof(RabbitCommand), publisher);
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ev));
        }

        #endregion

        #region Start

        [Fact]
        public async Task RabbitMQSubscriber_Should_Listen_To_AllExistingExchange_WithDefaultConfiguration()
        {
            try
            {
                CreateExchanges();
                var messages = new List<object>();
                var config = RabbitSubscriberConfiguration.GetDefault(subscriberName, GetConnectionFactory());
                config.ExchangeConfigurations.DoForEach(e =>
                    e.QueueConfiguration = new QueueConfiguration(new JsonDispatcherSerializer(), QueueName, callback: (e) => messages.Add(e)));

                var subscriber = new RabbitMQSubscriber(
                    _loggerFactory,
                    new RabbitMQSubscriberClientConfiguration(subscriberName, GetConnectionFactory(), config));

                subscriber.Start();

                var eventFromOne = GetEnveloppeDataForEvent(publisher1Name, "test 1 evt");
                var cmdFromOne = GetEnveloppeDataForCommand(publisher1Name, "test 1 cmd");
                var eventFromTwo = GetEnveloppeDataForEvent(publisher2Name, "test 2 evt");
                var cmdFromTwo = GetEnveloppeDataForCommand(publisher2Name, "test 2 cmd");

                _channel.BasicPublish(firstProducerEventExchangeName, "", body: eventFromOne);
                _channel.BasicPublish(secondProducerEventExchangeName, "", body: eventFromTwo);
                _channel.BasicPublish(firstProducerCommandExchangeName, subscriberName, body: cmdFromOne);
                _channel.BasicPublish(secondProducerCommandExchangeName, subscriberName, body: cmdFromTwo);

                uint spentTime = 0;
                while (messages.Count < 4 && spentTime < 2000)
                {
                    spentTime += 50;
                    await Task.Delay(50);
                }

                messages.Should().HaveCount(4);
                messages.Count(e => e.GetType() == typeof(RabbitEvent)).Should().Be(2);
                messages.Count(e => e.GetType() == typeof(RabbitCommand)).Should().Be(2);

            }
            finally
            {
                DeleteData();
            }
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
        public async Task RabbitMQSubscriber_Should_Listen_To_Configuration_Defined_Exchanges_And_Dispatch_InMemory()
        {
            try
            {
                CreateExchanges();
                var messages = new List<object>();
                var config = RabbitSubscriberConfiguration.GetDefault(subscriberName, GetConnectionFactory());
                config.ExchangeConfigurations.DoForEach(e =>
                    e.QueueConfiguration =
                        new QueueConfiguration(
                            new JsonDispatcherSerializer(), QueueName,
                            dispatchInMemory: true,
                            callback: (e) => messages.Add(e)));

                var subscriber = new RabbitMQSubscriber(
                    _loggerFactory,
                    new RabbitMQSubscriberClientConfiguration(subscriberName, GetConnectionFactory(), config),
                    () => new InMemory.Events.InMemoryEventBus());

                subscriber.Start();

                var evt = new RabbitEvent { Data = "data" };

                _channel.BasicPublish(
                    firstProducerEventExchangeName,
                    "",
                    body: Encoding.UTF8.GetBytes(
                            JsonConvert.SerializeObject(
                                new Enveloppe(
                                    JsonConvert.SerializeObject(evt), typeof(RabbitEvent), publisher1Name))));

                bool isFired = false;
                string data = "";
                RabbitHandler.OnEventArrived += (e) =>
                {
                    isFired = true;
                    data = e.Data;
                };
                ushort spentTime = 0;
                while (!isFired && spentTime < 2000)
                {
                    spentTime += 50;
                    await Task.Delay(50);
                }
                isFired.Should().BeTrue();
                data.Should().Be("data");
            }
            finally
            {
                DeleteData();
            }
        }

        #region AckStrategy

        private class AutoAckEvent : BaseDomainEvent { }
        private class ExceptionEvent : BaseDomainEvent { }
        private class AutoAckEventHandler : IDomainEventHandler<AutoAckEvent>
        {
            public Task<Result> HandleAsync(AutoAckEvent domainEvent, IEventContext context = null)
                => Result.Ok();
        }
        private class ExceptionEventHandler : IDomainEventHandler<ExceptionEvent>
        {
            public Task<Result> HandleAsync(ExceptionEvent domainEvent, IEventContext context = null)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task RabbitMQSubscriber_Should_Consider_AckStrategy_Ack_On_Success()
        {
            try
            {
                CreateExchanges();
                var messages = new List<object>();
                var config = RabbitSubscriberConfiguration.GetDefault(subscriberName, GetConnectionFactory());
                config.ExchangeConfigurations.DoForEach(e =>
                    e.QueueConfiguration =
                        new QueueConfiguration(
                            new JsonDispatcherSerializer(), QueueNameAckStrategy,
                            dispatchInMemory: true,
                            callback: (e) => messages.Add(e),
                            createAndUseDeadLetterQueue: true));
                config.AckStrategy = AckStrategy.AckOnSucces;
                var bus = new InMemory.Events.InMemoryEventBus(new InMemory.Events.InMemoryEventBusConfiguration
                {
                    NbRetries = 0
                });

                var subscriber = new RabbitMQSubscriber(
                    _loggerFactory,
                    new RabbitMQSubscriberClientConfiguration(subscriberName, GetConnectionFactory(), config),
                    () => bus);

                subscriber.Start();

                var evt = new AutoAckEvent();

                _channel.BasicPublish(
                    firstProducerEventExchangeName,
                    "",
                    body: Encoding.UTF8.GetBytes(
                            JsonConvert.SerializeObject(
                                new Enveloppe(
                                    JsonConvert.SerializeObject(evt), typeof(AutoAckEvent), publisher1Name))));
                await Task.Delay(100);
                var result = _channel.BasicGet(Consts.CONST_DEAD_LETTER_QUEUE_NAME, true);
                result.Should().BeNull();
            }
            finally
            {
                DeleteData();
            }
        }

        [Fact]
        public async Task RabbitMQSubscriber_Should_Consider_AckStrategy_Ack_On_Success_Fail_Should_Move_To_DLQ()
        {
            try
            {
                CreateExchanges();
                var messages = new List<object>();
                var config = RabbitSubscriberConfiguration.GetDefault(subscriberName, GetConnectionFactory());
                config.ExchangeConfigurations.DoForEach(e =>
                    e.QueueConfiguration =
                        new QueueConfiguration(
                            new JsonDispatcherSerializer(), QueueNameAckStrategy,
                            dispatchInMemory: true,
                            callback: (e) => messages.Add(e),
                            createAndUseDeadLetterQueue: true));
                config.AckStrategy = AckStrategy.AckOnSucces;

                var bus = new InMemory.Events.InMemoryEventBus(new InMemory.Events.InMemoryEventBusConfiguration
                {
                    NbRetries = 0
                });

                var subscriber = new RabbitMQSubscriber(
                    _loggerFactory,
                    new RabbitMQSubscriberClientConfiguration(subscriberName, GetConnectionFactory(), config),
                    () => bus);

                subscriber.Start();

                var evt = new ExceptionEvent();

                _channel.BasicPublish(
                    firstProducerEventExchangeName,
                    "",
                    body: Encoding.UTF8.GetBytes(
                            JsonConvert.SerializeObject(
                                new Enveloppe(
                                    JsonConvert.SerializeObject(evt), typeof(ExceptionEvent), publisher1Name))));
                await Task.Delay(250);
                var result = _channel.BasicGet(Consts.CONST_DEAD_LETTER_QUEUE_NAME, true);
                result.Should().NotBeNull();
            }
            finally
            {
                DeleteData();
            }
        }

        [Fact]
        public async Task RabbitMQSubscriber_Should_Consider_AckStrategy_Ack_On_Receive_Fail_Should_Remove_MessageFromQueue()
        {
            try
            {
                CreateExchanges();
                var messages = new List<object>();
                var config = RabbitSubscriberConfiguration.GetDefault(subscriberName, GetConnectionFactory());
                config.ExchangeConfigurations.DoForEach(e =>
                    e.QueueConfiguration =
                        new QueueConfiguration(
                            new JsonDispatcherSerializer(), QueueNameAckStrategy,
                            dispatchInMemory: true,
                            callback: (e) => messages.Add(e),
                            createAndUseDeadLetterQueue: true));
                config.AckStrategy = AckStrategy.AckOnReceive;
                var bus = new InMemory.Events.InMemoryEventBus(new InMemory.Events.InMemoryEventBusConfiguration
                {
                    NbRetries = 0
                });
                var subscriber = new RabbitMQSubscriber(
                    _loggerFactory,
                    new RabbitMQSubscriberClientConfiguration(subscriberName, GetConnectionFactory(), config),
                    () => bus);

                subscriber.Start();

                var evt = new ExceptionEvent();

                _channel.BasicPublish(
                    firstProducerEventExchangeName,
                    "",
                    body: Encoding.UTF8.GetBytes(
                            JsonConvert.SerializeObject(
                                new Enveloppe(
                                    JsonConvert.SerializeObject(evt), typeof(ExceptionEvent), publisher1Name))));
                await Task.Delay(250);
                var result = _channel.BasicGet(Consts.CONST_DEAD_LETTER_QUEUE_NAME, true);
                result.Should().BeNull();
            }
            finally
            {
                DeleteData();
            }
        }

        #endregion

    }

    #endregion

}

