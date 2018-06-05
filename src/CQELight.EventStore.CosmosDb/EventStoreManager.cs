using CQELight.Abstractions.Events.Interfaces;
using CQELight.Dispatcher;
using CQELight.IoC;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CQELight.EventStore.CosmosDb
{
    internal static class EventStoreManager
    {
        #region Internal static properties
        
        private static readonly ILogger _logger;

        #endregion

        #region Static accessor

        static EventStoreManager()
        {
            if (DIManager.IsInit)
            {
                _logger = DIManager.BeginScope().Resolve<ILoggerFactory>()?.CreateLogger("EventStore");
            }
            else
            {
                _logger = new LoggerFactory()
                    .AddDebug()
                    .CreateLogger(nameof(EventStoreManager));
            }
        }

        #endregion

        #region Public static methods

        internal static void Activate() => CoreDispatcher.OnEventDispatched += OnEventDispatchedMethod;

        internal static void Deactivate() => CoreDispatcher.OnEventDispatched -= OnEventDispatchedMethod;

        internal static async Task OnEventDispatchedMethod(IDomainEvent @event)
        {
            try
            {
                await new CosmosDbEventStore().StoreDomainEventAsync(@event).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                _logger?.LogError($"CosmosDb.EventHandler.OnEventDispatchedMethod() : Exception {exc}");
            }
        }

        #endregion

    }
}
