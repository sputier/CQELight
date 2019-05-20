using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.InMemory.Events;
using CQELight.Tools.Extensions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly string _emiter;
        private readonly ILogger _logger;
        private readonly InMemoryEventBus _inMemoryEventBus;

        #endregion

        #region Properties

        #endregion

        #region Ctor

        public AzureServiceBusServer(string emiter, ILoggerFactory loggerFactory,
            AzureServiceBusServerConfiguration configuration, InMemoryEventBus inMemoryEventBus = null)
        {
            if (string.IsNullOrWhiteSpace(emiter))
            {
                throw new ArgumentNullException(nameof(emiter));
            }
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _emiter = emiter;
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
                                await _inMemoryEventBus.PublishEventAsync(eventInstance).ConfigureAwait(false);
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
                var eventType = message?.ContentType;
                _logger.LogWarning("AzureServiceBusServer : Empty message received or event fired by unknown model !" +
                    (!string.IsNullOrWhiteSpace(eventType) ? $"Event type : {eventType}" : ""));
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
