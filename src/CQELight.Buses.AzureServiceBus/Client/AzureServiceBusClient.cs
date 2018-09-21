using CQELight.Abstractions.Configuration;
using CQELight.Abstractions.Dispatcher;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Configuration;
using CQELight.Events.Serializers;
using CQELight.Tools;
using CQELight.Tools.Extensions;
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
        private readonly AppId _appId;
        private readonly IDispatcherSerializer _dispatcherSerializer;
        private readonly IQueueClient _queueClient;

        #endregion

        #region Ctor

        public AzureServiceBusClient(IAppIdRetriever appIdRetriever, IDispatcherSerializer dispatcherSerializer, IQueueClient queueClient,
            AzureServiceBusClientConfiguration configuration)
        {
            if (appIdRetriever == null)
            {
                throw new ArgumentNullException(nameof(appIdRetriever));
            }
            _dispatcherSerializer = dispatcherSerializer ?? throw new ArgumentNullException(nameof(dispatcherSerializer));
            _appId = appIdRetriever.GetAppId();
            _configuration = configuration;
            _queueClient = queueClient;
        }

        public AzureServiceBusClient(IAppIdRetriever appIdRetriever, IQueueClient queueClient, AzureServiceBusClientConfiguration configuration)
            : this(appIdRetriever: appIdRetriever, dispatcherSerializer: new JsonDispatcherSerializer(), queueClient: queueClient, configuration: configuration)
        { }

        public AzureServiceBusClient(IAppIdRetriever appIdRetriever, AzureServiceBusClientConfiguration configuration)
            : this(appIdRetriever: appIdRetriever, dispatcherSerializer: new JsonDispatcherSerializer(), queueClient: AzureServiceBusContext.AzureQueueClient,
                  configuration: configuration)
        { }

        #endregion

        #region IDomainEventBus methods


        public Task PublishEventAsync(IDomainEvent @event, IEventContext context = null)
        {
            var eventType = @event.GetType();
            var lifetime = _configuration
                .EventsLifetime
                .FirstOrDefault(e => new TypeEqualityComparer().Equals(e.EventType, eventType))
                .LifeTime;
            return _queueClient.SendAsync(new Message
            {
                ContentType = @event.GetType().AssemblyQualifiedName,
                Body = Encoding.UTF8.GetBytes(_dispatcherSerializer.SerializeEvent(@event)),
                TimeToLive = lifetime.TotalSeconds > 0 ? lifetime : TimeSpan.FromDays(1),
                ReplyTo = _appId.ToString(),

            });
        }

        #endregion

    }
}
