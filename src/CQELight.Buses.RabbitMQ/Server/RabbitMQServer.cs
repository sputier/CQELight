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

namespace CQELight.Buses.RabbitMQ.Server
{
    /// <summary>
    /// Server instance to listen to RabbitMQ and apply callback.
    /// </summary>
    public class RabbitMQServer : DisposableObject
    {

        #region Static members

        /// <summary>
        /// Liste des types du système.
        /// </summary>
        private static List<Type> s_Types;

        #endregion

        #region Members

        /// <summary>
        /// Logger instance.
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// Connnection to RabbitMQ server.
        /// </summary>
        private IConnection _connection;
        /// <summary>
        /// Model for RabbitMQ communication.
        /// </summary>
        private IModel _eventChannel;
        /// <summary>
        /// Basic consummer.
        /// </summary>
        private EventingBasicConsumer _eventConsumer;
        /// <summary>
        /// Callback to invoke when event is receveid.
        /// </summary>
        private readonly Action<IDomainEvent> _eventCallback;
        private readonly RabbitMQServerConfiguration _config;

        #endregion

        #region static accessor

        static RabbitMQServer()
        {
            s_Types = ReflectionTools.GetAllTypes().ToList();
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Creation of a new server for listening to RabbitMQ.
        /// </summary>
        /// <param name="eventCallback">Callback to invoke when event is retrieved.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="commandCallback">Callback to inovoke when a command is retrieved.</param>
        /// <param name="config">Configuration to use.</param>
        public RabbitMQServer(Action<IDomainEvent> eventCallback, 
            ILoggerFactory loggerFactory, RabbitMQServerConfiguration config = null)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            _eventCallback = eventCallback ?? throw new ArgumentNullException(nameof(eventCallback));
            _logger = loggerFactory.CreateLogger<RabbitMQServer>();
            _config = config ?? RabbitMQServerConfiguration.Default;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Démarrage de l'écoute du bus.
        /// </summary>
        public void Start()
        {
            var factory = new ConnectionFactory() { HostName = _config.Host };
            _connection = factory.CreateConnection();

            //Events
            _eventChannel = _connection.CreateModel();
            _eventChannel.QueueDeclare(
                            queue: _config.QueueName,
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);
            _eventChannel.ExchangeDeclare(exchange: Consts.CONST_EVENTS_EXCHANGE_NAME,
                                    type: ExchangeType.Fanout,
                                    durable: true,
                                    autoDelete: true);
            _eventChannel.QueueBind(_config.QueueName, Consts.CONST_EVENTS_EXCHANGE_NAME, Consts.CONST_EVENTS_ROUTING_KEY);

            _eventConsumer = new EventingBasicConsumer(_eventChannel);
            _eventConsumer.Received += OnEventReceived;
            _eventChannel.BasicConsume(queue: _config.QueueName,
                                 autoAck: true,
                                 consumer: _eventConsumer); 
        }

        #endregion

        #region Private methods
        
        /// <summary>
        /// Event fired when an event has been received.
        /// </summary>
        /// <param name="model">Model.</param>
        /// <param name="args">Arguments.</param>
        private async void OnEventReceived(object model, BasicDeliverEventArgs args)
        {
            if (args.Body?.Any() == true)
            {
                try
                {
                    var eventAsJson = Encoding.UTF8.GetString(args.Body);
                    var eventTypeQualified = Encoding.UTF8.GetString(args.BasicProperties.Headers[Consts.CONST_HEADER_KEY_EVENT_TYPE] as byte[]);
                    var eventType =
                        s_Types.FirstOrDefault(t => t.AssemblyQualifiedName == eventTypeQualified)
                        ??
                        Type.GetType(eventTypeQualified);
                    if (!string.IsNullOrWhiteSpace(eventAsJson) && eventType != null)
                    {
                        var @event = eventAsJson.FromJson(eventType) as IDomainEvent;
                        _eventCallback.Invoke(@event);
                    }
                }
                catch (Exception exc)
                {
                    _logger.LogErrorMultilines("RabbitMQServer : Error when treating event.", exc.ToString());
                }
            }
            else
            {
                _logger.LogWarning("RabbitMQServer : Empty message received !");
            }
        }

        #endregion

        #region Overriden methods

        /// <summary>
        /// Disposal of resources.
        /// </summary>
        /// <param name="disposing">Indicator of origin.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                _connection.Dispose();
                _eventChannel.Dispose();
                _eventConsumer.Received -= OnEventReceived;
            }
            catch
            {
                //Not throw exception on cleanup
            }
        }

        #endregion

    }
}
