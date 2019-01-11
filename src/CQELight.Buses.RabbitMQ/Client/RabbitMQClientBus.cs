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
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        internal RabbitMQClientBus(IAppIdRetriever appIdRetriever, IDispatcherSerializer serializer,
            RabbitMQClientBusConfiguration configuration = null, ILoggerFactory loggerFactory = null)
        {
            if (appIdRetriever == null)
            {
                throw new ArgumentNullException(nameof(appIdRetriever));
            }
            _configuration = configuration ?? RabbitMQClientBusConfiguration.Default;
            _serializer = serializer ?? throw new System.ArgumentNullException(nameof(serializer));
            _appId = appIdRetriever.GetAppId();
            _logger = (loggerFactory ?? new LoggerFactory().AddDebug()).CreateLogger<RabbitMQClientBus>();
        }

        #endregion

        #region IDomainEventBus

        /// <summary>
        /// Register asynchronously an event to be processed by the bus.
        /// </summary>
        /// <param name="event">Event to register.</param>
        /// <param name="context">Context associated to the event.</param>
        public Task PublishEventAsync(IDomainEvent @event, IEventContext context = null)
        {
            var eventType = @event.GetType();
            if (@event != null)
            {
                _logger.LogDebug($"RabbitMQClientBus : Beginning of publishing event of type {eventType.FullName}");
                return Publish(GetEnveloppeFromEvent(@event));
            }
            _logger.LogDebug($"RabbitMQClientBus : End of publishing event of type {eventType.FullName}");
            return Task.CompletedTask;
        }

        public async Task PublishEventRangeAsync(IEnumerable<(IDomainEvent @event, IEventContext context)> data)
        {
            _logger.LogInformation($"RabbitMQClientBus : Beginning of treating bunch of events");
            var eventsGroup = data.GroupBy(d => d.@event.GetType())
                .Select(g => new
                {
                    Type = g.Key,
                    EventsAndContext = g.OrderBy(e => e.@event.EventTime).Select(e => (e.@event, e.context)).ToList()
                }).ToList();

#pragma warning disable 
            Task.Run(() =>
            {
                _logger.LogDebug($"RabbitMQClientBus : Found {eventsGroup.Count} group(s) :");
                eventsGroup.ForEach(e => _logger.LogDebug($"\t Event of type {e.Type} : {e.EventsAndContext.Count} events"));
            });
#pragma warning restore

            var tasks = new List<Task>();
            foreach (var item in eventsGroup)
            {
                var allowParallelDispatch = _configuration.ParallelDispatchEventTypes.Any(t => new TypeEqualityComparer().Equals(item.Type, t));
                tasks.Add(Task.Run(async () =>
                {
                    if (allowParallelDispatch)
                    {
                        _logger.LogInformation($"RabbitMQClientBus : Beginning of parallel dispatching events of type {item.Type.FullName}");
                        var innerTasks = new List<Task<Enveloppe>>();
                        foreach (var (@event, context) in item.EventsAndContext)
                        {
                            innerTasks.Add(Task.Run(() => GetEnveloppeFromEvent(@event)));
                        }
                        await Task.WhenAll(innerTasks).ConfigureAwait(false);
                        var enveloppes = innerTasks.Select(e => e.Result);
                        using (var connection = GetConnection())
                        {
                            using (var channel = GetChannel(connection))
                            {
                                var batch = channel.CreateBasicPublishBatch();
                                foreach (var env in enveloppes)
                                {
                                    var body = Encoding.UTF8.GetBytes(env.ToJson());
                                    var props = GetBasicProperties(channel, env);
                                    batch.Add(exchange: Consts.CONST_CQE_EXCHANGE_NAME,
                                                     routingKey: Consts.CONST_ROUTING_KEY_ALL,
                                                     mandatory: true,
                                                     properties: props,
                                                     body: body);
                                }
                                batch.Publish();
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"RabbitMQClientBus : Beginning of single op dispatching events of type {item.Type.FullName}");
                        foreach (var evtData in item.EventsAndContext)
                        {
                            await PublishEventAsync(evtData.@event, evtData.context).ConfigureAwait(false);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

        }

        #endregion

        #region Private methods

        private Enveloppe GetEnveloppeFromEvent(IDomainEvent @event)
        {
            var eventType = @event.GetType();
            var evtCfg = _configuration.EventsLifetime.FirstOrDefault(t => new TypeEqualityComparer().Equals(t.EventType, @event.GetType()));
            TimeSpan? expiration = null;
            if (evtCfg.LifeTime.TotalMilliseconds > 0)
            {
                expiration = evtCfg.LifeTime;
                _logger.LogDebug($"RabbitMQClientBus : Defining {evtCfg.LifeTime.ToString()} lifetime for event of type {eventType.FullName}");
            }
            var serializedEvent = _serializer.SerializeEvent(@event);
            if (expiration.HasValue)
            {
                return new Enveloppe(serializedEvent, eventType, _appId, true, expiration.Value);
            }
            return new Enveloppe(serializedEvent, eventType, _appId);

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
