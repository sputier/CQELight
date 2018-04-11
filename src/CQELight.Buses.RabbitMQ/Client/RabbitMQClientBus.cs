using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Tools.Extensions;
using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Dispatcher;

namespace CQELight.Buses.RabbitMQ.Client
{
    /// <summary>
    /// RabbitMQ client bus instance. It uses its configuration to push to a RabbitMQ instance.
    /// </summary>
    public class RabbitMQClientBus : IDomainEventBus, ICommandBus
    {

        #region Members

        private static RabbitMQClientBusConfiguration _configuration;
        private readonly IDispatcherSerializer _serializer;

        #endregion

        #region Ctor

        public RabbitMQClientBus(IDispatcherSerializer serializer, RabbitMQClientBusConfiguration configuration = null)
        {
            _configuration = configuration ?? RabbitMQClientBusConfiguration.Default;
            _serializer = serializer ?? throw new System.ArgumentNullException(nameof(serializer));
        }

        #endregion

        #region IDomainEventBus

        public Task RegisterAsync(IDomainEvent @event, IEventContext context = null)
        {
            if (@event != null)
            {
                var connection = GetConnection();
                var channel = GetChannel(connection);
                var body = Encoding.UTF8.GetBytes(_serializer.SerializeEvent(@event));

                IBasicProperties props = channel.CreateBasicProperties();
                props.ContentType = _serializer.ContentType;
                props.DeliveryMode = 2;
                props.Type = @event.GetType().AssemblyQualifiedName;

                return Task.Run(() => channel.BasicPublish(
                                     exchange: Consts.CONST_EVENTS_EXCHANGE_NAME,
                                     routingKey: Consts.CONST_EVENTS_ROUTING_KEY,
                                     basicProperties: props,
                                     body: body))
                 .ContinueWith(t =>
                                     {
                                         try
                                         {
                                             connection.Dispose();
                                             channel.Dispose();
                                         }
                                         catch
                                         {

                                         }
                                     });
            }
            return Task.CompletedTask;
        }

        #endregion

        #region ICommandBus

        /// <summary>
        /// Dispatch command asynchrounously.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="context">Context associated to command.</param>
        /// <returns>List of launched tasks from handler.</returns>
        public Task<Task[]> DispatchAsync(ICommand command, ICommandContext context = null)
        {
            throw new System.NotImplementedException();
        }


        #endregion

        #region Private methods

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

        private IModel GetChannel(IConnection connection, bool forEvent = true)
        {
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: forEvent ? Consts.CONST_EVENTS_EXCHANGE_NAME : Consts.CONST_COMMANDS_EXCHANGE_NAME,
                                    type: ExchangeType.Fanout,
                                    durable: true,
                                    autoDelete: false);
            return channel;
        }

        #endregion

    }
}
