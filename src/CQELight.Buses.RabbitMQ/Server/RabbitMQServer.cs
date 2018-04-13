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

        private readonly Func<IDomainEvent, Task> _eventAsyncCallback;
        private readonly RabbitMQServerConfiguration _config;

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
            var connection = GetConnection();
            var channel = GetChannel(connection);

            channel.ExchangeDeclare(exchange: Consts.CONSTS_CQE_EXCHANGE_NAME,
                                        type: ExchangeType.Fanout,
                                        durable: true,
                                        autoDelete: true);
            foreach (var queue in _config.QueuesConfiguration)
            {
                channel.QueueDeclare(
                                queue: queue.QueueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments:
                                queue.CreateAndUseDeadLetterQueue
                                    ? new Dictionary<string, object> { ["x-dead-letter-exchange"] = $"cqe_dead_letter_{queue.QueueName}" }
                                    : null);
                if (queue.CreateAndUseDeadLetterQueue)
                {
                    channel.QueueDeclare(
                                    queue: $"cqe_dead_letter_{queue.QueueName}",
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false);
                }
                channel.QueueBind(queue.QueueName, Consts.CONSTS_CQE_EXCHANGE_NAME, Consts.CONST_EVENTS_ROUTING_KEY);

                var eventConsumer = new EventingBasicConsumer(channel);
                eventConsumer.Received += OnEventReceived;
                channel.BasicConsume(queue: queue.QueueName,
                                     autoAck: false,
                                     consumer: eventConsumer);
            }
        }

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
            if (args.Body?.Any() == true && model is DefaultBasicConsumer consumer)
            {
                try
                {
                    var eventAsStr = Encoding.UTF8.GetString(args.Body);
                    var eventTypeQualified = args.BasicProperties.Type;
                    var eventType = Type.GetType(eventTypeQualified);
                    if (!string.IsNullOrWhiteSpace(eventAsStr) && eventType != null)
                    {
                        var @event = eventAsStr.FromJson(eventType) as IDomainEvent;
                        await _eventAsyncCallback.Invoke(@event);
                        consumer.Model.BasicAck(args.DeliveryTag, false);
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
            //try
            //{
            //    if (_config.DeleteQueueOnDispose)
            //    {
            //        eventChannel.QueueDelete(_config.QueueName);
            //    }
            //    connection.Dispose();
            //    eventChannel.Dispose();
            //    eventConsumer.Received -= OnEventReceived;
            //}
            //catch
            //{
            //    //Not throw exception on cleanup
            //}
        }

        #endregion

    }
}
