using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Tools.Extensions;

namespace CQELight.Buses.RabbitMQ.Events
{
    /// <summary>
    /// Event bus that dispatch events into a RabbitMQ Instance.
    /// </summary>
    public class RabbitMQClientEventBus : IDomainEventBus, IConfigurableBus<RabbitMQClientEventBusConfiguration>
    {

        #region Members

        /// <summary>
        /// Current configuration.
        /// </summary>
        private static RabbitMQClientEventBusConfiguration _configuration;

        #endregion

        #region IDomainEventBus

        /// <summary>
        /// Register asynchronously an event to be processed by the bus.
        /// </summary>
        /// <param name="event">Event to register.</param>
        /// <param name="context">Context associated to the event..</param>
        public Task RegisterAsync(IDomainEvent @event, IEventContext context = null)
        {
            if (@event != null)
            {
                var factory = new ConnectionFactory() { HostName = _configuration.Host };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: Consts.CONST_EVENTS_EXCHANGE_NAME,
                                                type: ExchangeType.Fanout,
                                                durable: true,
                                                autoDelete: true);

                        var body = Encoding.UTF8.GetBytes(@event.ToJson());

                        IBasicProperties props = channel.CreateBasicProperties();
                        props.ContentType = "application/json";
                        props.DeliveryMode = 2;
                        props.Headers = new Dictionary<string, object>();
                        props.Headers.Add(Consts.CONST_HEADER_KEY_EVENT_TYPE, @event.GetType().AssemblyQualifiedName);

                        channel.BasicPublish(exchange: Consts.CONST_EVENTS_EXCHANGE_NAME,
                                             routingKey: Consts.CONST_EVENTS_ROUTING_KEY,
                                             basicProperties: props,
                                             body: body);
                    }
                }
            }
            return Task.CompletedTask;
        }

        #endregion

        #region IConfigurableBus

        /// <summary>
        /// Apply the configuration to the bus.
        /// </summary>
        /// <param name="config">Bus configuration.</param>
        public void Configure(RabbitMQClientEventBusConfiguration config)
            => _configuration = config;

        #endregion

    }
}
