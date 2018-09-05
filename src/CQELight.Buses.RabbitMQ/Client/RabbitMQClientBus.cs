using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Tools.Extensions;
using CQELight.Abstractions.Dispatcher;
using System;
using System.Linq;
using CQELight.Tools;
using CQELight.Configuration;
using CQELight.Abstractions.Configuration;
using CQELight.Buses.RabbitMQ.Extensions;

namespace CQELight.Buses.RabbitMQ.Client
{
    /// <summary>
    /// RabbitMQ client bus instance. It uses its configuration to push to a RabbitMQ instance.
    /// </summary>
    public class RabbitMQClientBus : IDomainEventBus
    {
        #region Members

        private static RabbitMQClientBusConfiguration _configuration;
        private readonly IDispatcherSerializer _serializer;
        private readonly AppId _appId;

        #endregion

        #region Ctor

        internal RabbitMQClientBus(IAppIdRetriever appIdRetriever, IDispatcherSerializer serializer, RabbitMQClientBusConfiguration configuration = null)
        {
            if (appIdRetriever == null)
            {
                throw new ArgumentNullException(nameof(appIdRetriever));
            }
            _configuration = configuration ?? RabbitMQClientBusConfiguration.Default;
            _serializer = serializer ?? throw new System.ArgumentNullException(nameof(serializer));
            _appId = appIdRetriever.GetAppId();
        }

        #endregion

        #region IDomainEventBus

        /// <summary>
        /// Register asynchronously an event to be processed by the bus.
        /// </summary>
        /// <param name="event">Event to register.</param>
        /// <param name="context">Context associated to the event.</param>
        public Task RegisterAsync(IDomainEvent @event, IEventContext context = null)
        {
            if (@event != null)
            {
                var evtCfg = _configuration.EventsLifetime.FirstOrDefault(t => new TypeEqualityComparer().Equals(t.Type, @event.GetType()));
                TimeSpan? expiration = null;
                if (evtCfg.Expiration.TotalMilliseconds > 0)
                {
                    expiration = evtCfg.Expiration;
                }
                var serializedEvent = _serializer.SerializeEvent(@event);
                return Publish(expiration.HasValue
                    ? new Enveloppe(serializedEvent, @event.GetType(), _appId, true, expiration.Value)
                    : new Enveloppe(serializedEvent, @event.GetType(), _appId));
            }
            return Task.CompletedTask;
        }

        #endregion

        #region Private methods

        private Task Publish(Enveloppe env)
        {
            using (var connection = GetConnection())
            {
                using (var channel = GetChannel(connection))
                {
                    var body = Encoding.UTF8.GetBytes(env.ToJson());

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/json";
                    props.DeliveryMode = (byte)(env.PersistentMessage ? 2 : 1);
                    props.Type = env.AssemblyQualifiedDataType;
                    channel.BasicPublish(
                                         exchange: Consts.CONST_CQE_EXCHANGE_NAME,
                                         routingKey: Consts.CONST_ROUTING_KEY_ALL,
                                         basicProperties: props,
                                         body: body);
                }
            }
            return Task.CompletedTask;
        }

        private IConnection GetConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration.Host,
                UserName = _configuration.UserName,
                Password = _configuration.Password
            };
            if (_configuration.Port.HasValue)
            {
                factory.Port = _configuration.Port.Value;
            }
            return factory.CreateConnection();
        }

        private IModel GetChannel(IConnection connection)
        {
            string queueName = _appId.ToQueueName();
            var channel = connection.CreateModel();
            channel.CreateCQEExchange();

            channel.QueueDeclare(
                           queue: queueName,
                           durable: true,
                           exclusive: false,
                           autoDelete: false);
            channel.QueueBind(queueName, Consts.CONST_CQE_EXCHANGE_NAME, Consts.CONST_ROUTING_KEY_ALL);
            channel.QueueBind(queueName, Consts.CONST_CQE_EXCHANGE_NAME, _appId.Value.ToString());

            return channel;
        }

        #endregion

    }
}
