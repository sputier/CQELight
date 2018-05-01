using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Buses.RabbitMQ.Server
{
    /// <summary>
    /// Server instance to listen to RabbitMQ and apply callback.
    /// </summary>
    public class RabbitMQServer : DisposableObject
    {

        #region Members

        private readonly ILogger _logger;
        private readonly RabbitMQServerConfiguration _config;
        private List<EventingBasicConsumer> _consumers = new List<EventingBasicConsumer>();
        private IConnection _connection;
        private IModel _channel;

        #endregion

        #region Ctor

        internal RabbitMQServer(ILoggerFactory loggerFactory, RabbitMQServerConfiguration config = null)
        {
            _logger = (loggerFactory ?? new LoggerFactory().AddDebug()).CreateLogger<RabbitMQServer>();
            _config = config ?? RabbitMQServerConfiguration.Default;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Beginning server.
        /// </summary>
        public void Start()
        {
            _consumers = new List<EventingBasicConsumer>();
            _connection = GetConnection();
            _channel = GetChannel(_connection);

            _channel.ExchangeDeclare(exchange: Consts.CONSTS_CQE_EXCHANGE_NAME,
                                        type: ExchangeType.Fanout,
                                        durable: true,
                                        autoDelete: false);
            foreach (var queueConfig in _config.QueuesConfiguration)
            {
                _channel.QueueDeclare(
                                queue: queueConfig.QueueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments:
                                queueConfig.CreateAndUseDeadLetterQueue
                                    ? new Dictionary<string, object> { ["x-dead-letter-exchange"] = $"{Consts.CONST_QUEUE_DEAD_LETTER_QUEUE_PREFIX}{queueConfig.QueueName}" }
                                    : null);
                if (queueConfig.CreateAndUseDeadLetterQueue)
                {
                    _channel.QueueDeclare(
                                    queue: $"{Consts.CONST_QUEUE_DEAD_LETTER_QUEUE_PREFIX}{queueConfig.QueueName}",
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false);
                }
                _channel.QueueBind(queueConfig.QueueName, Consts.CONSTS_CQE_EXCHANGE_NAME, queueConfig.RoutingKey);

                var queueConsumer = new CQEBasicConsumer(_channel, queueConfig);
                queueConsumer.Received += OnEventReceived;
                _channel.BasicConsume(queue: queueConfig.QueueName,
                                     autoAck: false,
                                     consumer: queueConsumer);
                _consumers.Add(queueConsumer);
            }
        }

        /// <summary>
        /// Stop the server and cleanup resources.
        /// </summary>
        public void Stop()
            => Dispose();

        #endregion

        #region Private methods

        private IConnection GetConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _config.Host,
                UserName = _config.UserName,
                Password = _config.Password
            };
            if (_config.Port.HasValue)
            {
                factory.Port = _config.Port.Value;
            }
            return factory.CreateConnection();
        }

        private IModel GetChannel(IConnection connection)
        {
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: Consts.CONSTS_CQE_EXCHANGE_NAME,
                                    type: ExchangeType.Fanout,
                                    durable: true,
                                    autoDelete: false);
            return channel;
        }

        private async void OnEventReceived(object model, BasicDeliverEventArgs args)
        {
            if (args.Body?.Any() == true && model is CQEBasicConsumer consumer)
            {
                try
                {
                    var dataAsStr = Encoding.UTF8.GetString(args.Body);
                    var dataQualifiedType = args.BasicProperties.Type;
                    var dataType = Type.GetType(dataQualifiedType);
                    if (!string.IsNullOrWhiteSpace(dataAsStr) && dataType != null)
                    {
                        if (args.RoutingKey == Consts.CONST_EVENTS_ROUTING_KEY)
                        {
                            var evt = consumer.Configuration.Serializer.DeserializeEvent(dataAsStr, dataType);
                            consumer.Configuration.Callback?.Invoke(evt);
                            if (consumer.Configuration.DispatchInMemory)
                            {

                            }
                            consumer.Model.BasicAck(args.DeliveryTag, false);
                        }
                        else if (args.RoutingKey == Consts.CONST_COMMANDS_ROUTING_KEY)
                        {
                            var cmd = consumer.Configuration.Serializer.DeserializeCommand(dataAsStr, dataType);
                            consumer.Configuration.Callback?.Invoke(cmd);
                            if (consumer.Configuration.DispatchInMemory)
                            {

                            }
                            consumer.Model.BasicAck(args.DeliveryTag, false);
                        }
                    }
                }
                catch (Exception exc)
                {
                    _logger.LogErrorMultilines("RabbitMQServer : Error when treating event.", exc.ToString());
                    consumer.Model.BasicReject(args.DeliveryTag, false); //TODO make it configurable for retry or so
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
                _consumers.DoForEach(c =>
                {
                    c.Received -= OnEventReceived;
                });
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
