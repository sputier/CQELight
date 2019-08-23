using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Dispatcher;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Buses.RabbitMQ.Client
{
    public class RabbitMQCommandBus : ICommandBus
    {
        #region Members

        private readonly RabbitMQClientBusConfiguration _configuration;
        private readonly IDispatcherSerializer _serializer;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public RabbitMQCommandBus(
            IDispatcherSerializer serializer,
            RabbitMQClientBusConfiguration configuration,
            ILoggerFactory loggerFactory = null)
        {
            if(loggerFactory == null)
            {
                loggerFactory = new LoggerFactory();
                loggerFactory.AddProvider(new DebugLoggerProvider());
            }
            _logger = loggerFactory.CreateLogger<RabbitMQCommandBus>();
            _serializer = serializer;
            _configuration = configuration;
        }

        #endregion

        #region ICommand bus methods

        public async Task<Result> DispatchAsync(ICommand command, ICommandContext context = null)
        {
            if (command != null)
            {
                var commandType = command.GetType();
                _logger.LogDebug($"RabbitMQClientBus : Beginning of publishing command of type {commandType.FullName}");
                await Publish(GetEnveloppeForCommand(command)).ConfigureAwait(false);
                _logger.LogDebug($"RabbitMQClientBus : End of publishing command of type {commandType.FullName}");
                return Result.Ok();
            }
            return Result.Fail("RabbitMQClientBus : No command provided to publish method");
        }

        #endregion

        #region Private methods

        private Enveloppe GetEnveloppeForCommand(ICommand command)
        {
            var eventType = command.GetType();
            var evtCfg = _configuration.EventsLifetime.FirstOrDefault(t => new TypeEqualityComparer().Equals(t.EventType, command.GetType()));
            TimeSpan? expiration = null;
            if (evtCfg.LifeTime.TotalMilliseconds > 0)
            {
                expiration = evtCfg.LifeTime;
                _logger.LogDebug($"RabbitMQClientBus : Defining {evtCfg.LifeTime.ToString()} lifetime for event of type {eventType.FullName}");
            }
            var serializedEvent = _serializer.SerializeCommand(command);
            if (expiration.HasValue)
            {
                return new Enveloppe(serializedEvent, eventType, _configuration.Emiter, true, expiration.Value);
            }
            return new Enveloppe(serializedEvent, eventType, _configuration.Emiter);
        }

        private IBasicProperties GetBasicProperties(IModel channel, Enveloppe env)
        {
            IBasicProperties props = channel.CreateBasicProperties();
            props.ContentType = "text/json";
            props.DeliveryMode = (byte)(env.PersistentMessage ? 2 : 1);
            props.Type = env.AssemblyQualifiedDataType;
            return props;
        }

        private Task Publish(Enveloppe env)
        {
            using (var connection = GetConnection())
            {
                using (var channel = GetChannel(connection))
                {
                    var body = Encoding.UTF8.GetBytes(env.ToJson());
                    var props = GetBasicProperties(channel, env);

                    channel.BasicPublish(
                                         exchange: _configuration.Emiter + "_commands",
                                         routingKey: env.AssemblyQualifiedDataType.Split('.')[0],
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
            var channel = connection.CreateModel();
            var exchangeName = _configuration.Emiter + "_commands";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, true);

            return channel;
        }
        #endregion
    }
}
