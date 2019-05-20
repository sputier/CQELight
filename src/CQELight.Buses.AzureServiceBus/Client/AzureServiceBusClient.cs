using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Dispatcher;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Events.Serializers;
using CQELight.Tools;
using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Buses.AzureServiceBus.Client
{

    class AzureServiceBusClient : IDomainEventBus
    {

        #region Members

        private readonly AzureServiceBusClientConfiguration _configuration;
        private readonly string _emiter;
        private readonly IDispatcherSerializer _dispatcherSerializer;
        private readonly IQueueClient _queueClient;

        #endregion

        #region Ctor

        public AzureServiceBusClient(string emiter, IDispatcherSerializer dispatcherSerializer, IQueueClient queueClient,
            AzureServiceBusClientConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(emiter))
            {
                throw new ArgumentNullException(nameof(emiter));
            }
            _dispatcherSerializer = dispatcherSerializer ?? throw new ArgumentNullException(nameof(dispatcherSerializer));
            _emiter = emiter;
            _configuration = configuration;
            _queueClient = queueClient;
        }

        public AzureServiceBusClient(string emiter, IQueueClient queueClient, AzureServiceBusClientConfiguration configuration)
            : this(emiter: emiter, dispatcherSerializer: new JsonDispatcherSerializer(), queueClient: queueClient, configuration: configuration)
        { }

        public AzureServiceBusClient(string emiter, AzureServiceBusClientConfiguration configuration)
            : this(emiter: emiter, dispatcherSerializer: new JsonDispatcherSerializer(), queueClient: AzureServiceBusContext.AzureQueueClient,
                  configuration: configuration)
        { }

        #endregion

        #region IDomainEventBus methods

        public async Task<Result> PublishEventAsync(IDomainEvent @event, IEventContext context = null)
        {
            try
            {
                var eventType = @event.GetType();
                var lifetime = _configuration
                    .EventsLifetime
                    .FirstOrDefault(e => new TypeEqualityComparer().Equals(e.EventType, eventType))
                    .LifeTime;
                await _queueClient.SendAsync(new Message
                {
                    ContentType = @event.GetType().AssemblyQualifiedName,
                    Body = Encoding.UTF8.GetBytes(_dispatcherSerializer.SerializeEvent(@event)),
                    TimeToLive = lifetime.TotalSeconds > 0 ? lifetime : TimeSpan.FromDays(1),
                    ReplyTo = _emiter.ToString(),

                }).ConfigureAwait(false);
                return Result.Ok();
            }
            catch // TODO Log exception
            {
                return Result.Fail();
            }
        }

        public async Task<Result> PublishEventRangeAsync(IEnumerable<IDomainEvent> events)
        {
            try
            {
                var messages = events.Select(c =>
                {
                    var eventType = c.GetType();
                    var lifetime = _configuration
                        .EventsLifetime
                        .FirstOrDefault(e => new TypeEqualityComparer().Equals(e.EventType, eventType))
                        .LifeTime;
                    return new Message
                    {
                        ContentType = c.GetType().AssemblyQualifiedName,
                        Body = Encoding.UTF8.GetBytes(_dispatcherSerializer.SerializeEvent(c)),
                        TimeToLive = lifetime.TotalSeconds > 0 ? lifetime : TimeSpan.FromDays(1),
                        ReplyTo = _emiter.ToString(),

                    };
                }).ToList();
                await _queueClient.SendAsync(messages).ConfigureAwait(false);
                return Result.Ok();
            }
            catch// TODO Log exception
            {
                return Result.Fail();
            }
        }

        #endregion

    }
}
