using CQELight.Abstractions.Configuration;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Configuration;
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

        #endregion

        #region Ctor

        public AzureServiceBusClient(IAppIdRetriever appIdRetriever, AzureServiceBusClientConfiguration configuration)
        {
            if (appIdRetriever == null)
            {
                throw new ArgumentNullException(nameof(appIdRetriever));
            }
            _appId = appIdRetriever.GetAppId();
            _configuration = configuration;
        }

        #endregion

        #region IDomainEventBus methods


        public Task RegisterAsync(IDomainEvent @event, IEventContext context = null)
        {
            var eventType = @event.GetType();
            var lifetime = _configuration
                .EventsLifetime
                .FirstOrDefault(e => new TypeEqualityComparer().Equals(e.EventType, eventType))
                .LifeTime;
            return AzureServiceBusContext.AzureQueueClient.SendAsync(new Message
            {
                ContentType = @event.GetType().AssemblyQualifiedName,
                Body = Encoding.UTF8.GetBytes(@event.ToJson()),
                TimeToLive = lifetime.TotalSeconds > 0 ? lifetime : TimeSpan.FromDays(1),
                ReplyTo = _appId.ToString(),
                
            });
        }

        #endregion

    }
}
