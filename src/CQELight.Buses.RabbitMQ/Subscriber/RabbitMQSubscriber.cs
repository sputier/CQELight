using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.InMemory.Commands;
using CQELight.Buses.InMemory.Events;
using CQELight.Buses.RabbitMQ.Extensions;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Subscriber
{
    /// <summary>
    /// Subscriber instance that will do callbacks when pumping messages from
    /// rabbit queue.
    /// </summary>
    public class RabbitMQSubscriber : DisposableObject
    {
        #region Members

        private static List<string> _rejectedDeliveryTags = new List<string>();
        private readonly ILogger _logger;
        private readonly RabbitMQSubscriberClientConfiguration _config;
        private List<EventingBasicConsumer> _consumers = new List<EventingBasicConsumer>();
        private IConnection _connection;
        private IModel _channel;
        private readonly Func<InMemoryEventBus> _inMemoryEventBusFactory;
        private readonly Func<InMemoryCommandBus> _inMemoryCommandBusFactory;

        #endregion

        #region Ctor

        public RabbitMQSubscriber(
            ILoggerFactory loggerFactory,
            RabbitMQSubscriberClientConfiguration config,
            Func<InMemoryEventBus> inMemoryEventBusFactory = null,
            Func<InMemoryCommandBus> inMemoryCommandBusFactory = null)
        {
            if (loggerFactory == null)
            {
                loggerFactory = new LoggerFactory();
                loggerFactory.AddProvider(new DebugLoggerProvider());
            }
            _logger = loggerFactory.CreateLogger<RabbitMQSubscriber>();
            _config = config;
            _inMemoryEventBusFactory = inMemoryEventBusFactory;
            _inMemoryCommandBusFactory = inMemoryCommandBusFactory;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Start the RabbitMQ in-app server.
        /// </summary>
        public void Start()
        {
            _consumers = new List<EventingBasicConsumer>();
            _connection = GetConnection();
            _channel = GetChannel(_connection);

            foreach (var exchangeConfig in _config.SubscriberConfiguration.ExchangeConfigurations)
            {
                if (exchangeConfig.QueueConfiguration.CreateAndUseDeadLetterQueue)
                {
                    _channel.ExchangeDeclare(
                        Consts.CONST_DEAD_LETTER_EXCHANGE_NAME,
                        ExchangeType.Fanout,
                        true,
                        false
                        );
                    _channel.QueueDeclare(
                        Consts.CONST_DEAD_LETTER_QUEUE_NAME,
                        durable: true,
                        exclusive: false,
                        autoDelete: false
                        );
                    _channel.QueueBind(Consts.CONST_DEAD_LETTER_QUEUE_NAME, Consts.CONST_DEAD_LETTER_EXCHANGE_NAME, "");
                }
                _channel.ExchangeDeclare(
                    exchangeConfig.ExchangeDetails.ExchangeName,
                    exchangeConfig.ExchangeDetails.ExchangeType,
                    exchangeConfig.ExchangeDetails.Durable,
                    exchangeConfig.ExchangeDetails.AutoDelete
                    );
                _channel.QueueDeclare(
                    exchangeConfig.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    exchangeConfig.QueueConfiguration.CreateAndUseDeadLetterQueue
                                ? new Dictionary<string, object> { ["x-dead-letter-exchange"] = Consts.CONST_DEAD_LETTER_EXCHANGE_NAME }
                                : null);
                _channel.QueueBind(exchangeConfig.QueueName, exchangeConfig.ExchangeDetails.ExchangeName, exchangeConfig.RoutingKey ?? "");

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += OnEventReceived;
                _channel.BasicConsume(exchangeConfig.QueueName, autoAck: false, consumer);
                _consumers.Add(consumer);
            }
        }

        /// <summary>
        /// Stop the server and cleanup resources.
        /// </summary>
        public void Stop()
            => Dispose();

        #endregion

        #region Private methods

        private IConnection GetConnection() => _config.ConnectionFactory.CreateConnection();

        private IModel GetChannel(IConnection connection) => connection.CreateModel();

        private async void OnEventReceived(object model, BasicDeliverEventArgs args)
        {
            if (args.Body?.Any() == true && model is EventingBasicConsumer consumer)
            {
                var result = Result.Ok();
                try
                {
                    var dataAsStr = Encoding.UTF8.GetString(args.Body);
                    var enveloppe = dataAsStr.FromJson<Enveloppe>();
                    if (enveloppe != null)
                    {
                        if (enveloppe.Emiter == _config.Emiter)
                        {
                            return;
                        }
                        if (!string.IsNullOrWhiteSpace(enveloppe.Data) && !string.IsNullOrWhiteSpace(enveloppe.AssemblyQualifiedDataType))
                        {
                            var objType = Type.GetType(enveloppe.AssemblyQualifiedDataType);
                            if (objType != null)
                            {
                                var exchangeConfig = _config.SubscriberConfiguration.ExchangeConfigurations.First(e => e.ExchangeDetails.ExchangeName == args.Exchange);
                                if (typeof(IDomainEvent).IsAssignableFrom(objType))
                                {
                                    var evt = exchangeConfig.QueueConfiguration.Serializer.DeserializeEvent(enveloppe.Data, objType);
                                    exchangeConfig.QueueConfiguration.Callback?.Invoke(evt);
                                    if (exchangeConfig.QueueConfiguration.DispatchInMemory && _inMemoryEventBusFactory != null)
                                    {
                                        var bus = _inMemoryEventBusFactory();
                                        result = await bus.PublishEventAsync(evt).ConfigureAwait(false);
                                    }
                                }
                                else if (typeof(ICommand).IsAssignableFrom(objType))
                                {
                                    var cmd = exchangeConfig.QueueConfiguration.Serializer.DeserializeCommand(enveloppe.Data, objType);
                                    exchangeConfig.QueueConfiguration.Callback?.Invoke(cmd);
                                    if (exchangeConfig.QueueConfiguration.DispatchInMemory && _inMemoryCommandBusFactory != null)
                                    {
                                        var bus = _inMemoryCommandBusFactory();
                                        result = await bus.DispatchAsync(cmd).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    _logger.LogErrorMultilines("RabbitMQServer : Error when treating event.", exc.ToString());
                    result = Result.Fail();
                }
                if (!result && _config.SubscriberConfiguration.AckStrategy == Configuration.AckStrategy.AckOnSucces)
                {
                    consumer.Model.BasicReject(args.DeliveryTag, false);
                }
                else
                {
                    consumer.Model.BasicAck(args.DeliveryTag, false);
                }
            }
            else
            {
                _logger.LogWarning("RabbitMQServer : Empty message received or event fired by bad model !");
            }
        }

        #endregion

        #region Overriden methods

        protected override void Dispose(bool disposing)
        {
            try
            {
                _channel.Dispose();
                _channel.Dispose();
                _consumers.DoForEach(c => c.Received -= OnEventReceived);
                _consumers.Clear();
            }
            catch
            {
                //Not throw exception on cleanup
            }
        }

        #endregion

    }
}
