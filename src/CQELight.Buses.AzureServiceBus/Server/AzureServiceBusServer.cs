using CQELight.Abstractions.Configuration;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.InMemory.Events;
using CQELight.Configuration;
using CQELight.Tools.Extensions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.Buses.AzureServiceBus.Server
{
    class AzureServiceBusServer
    {

        #region Members

        private readonly AzureServiceBusServerConfiguration _configuration;
        private readonly IQueueClient _client;
        private readonly AppId _appId;
        private readonly ILogger _logger;
        private readonly InMemoryEventBus _inMemoryEventBus;

        #endregion

        #region Properties

        #endregion

        #region Ctor

        public AzureServiceBusServer(IAppIdRetriever appIdRetriever, ILoggerFactory loggerFactory,
            AzureServiceBusServerConfiguration configuration, InMemoryEventBus inMemoryEventBus = null)
        {
            if (appIdRetriever == null)
            {
                throw new ArgumentNullException(nameof(appIdRetriever));
            }
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _appId = appIdRetriever.GetAppId();
            _logger = (loggerFactory ?? new LoggerFactory().AddDebug()).CreateLogger<AzureServiceBusServer>();

            _client = new QueueClient(configuration.ConnectionString, configuration.QueueConfiguration.QueueName);
            _client.RegisterMessageHandler(ReceiveMessageAsync, ReceiveMessageExceptionAsync);
            _inMemoryEventBus = inMemoryEventBus;
        }

        #endregion

        #region Public methods

        async Task ReceiveMessageAsync(Message message, CancellationToken token)
        {
            if (message?.Body != null)
            {
                try
                {
                    var bodyAsString = Encoding.UTF8.GetString(message.Body);
                    if (!string.IsNullOrWhiteSpace(bodyAsString))
                    {
                        var objType = Type.GetType(message.ContentType);
                        if (objType.GetInterfaces().Any(i => i.Name == nameof(IDomainEvent)))
                        {
                            var eventInstance = _configuration.QueueConfiguration.Serializer.DeserializeEvent(bodyAsString, objType);
                            _configuration.QueueConfiguration.Callback?.Invoke(eventInstance);
                            if (_configuration.QueueConfiguration.DispatchInMemory && _inMemoryEventBus != null)
                            {
                                await _inMemoryEventBus.PublishEventAsync(eventInstance);
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    _logger.LogErrorMultilines("AzureServiceBusServer : Error when treating event.", exc.ToString());
                }
            }
            else
            {
                _logger.LogWarning($"AzureServiceBusServer : Empty message received or event fired by unknown model ! Event type : {message.ContentType}");
            }
        }

        Task ReceiveMessageExceptionAsync(ExceptionReceivedEventArgs args)
        {
            _logger.LogErrorMultilines("AzureServiceBusServer : Error when receiving message.", args.Exception.ToString());
            return Task.CompletedTask;
        }

        #endregion

    }
}
