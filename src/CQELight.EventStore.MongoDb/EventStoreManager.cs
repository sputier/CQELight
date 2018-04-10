using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Dispatcher;
using CQELight.EventStore.MongoDb.Common;
using CQELight.IoC;
using CQELight.Tools.Extensions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.EventStore.MongoDb
{
    static class EventStoreManager
    {

        #region Internal static properties

        internal static string ServersUrls;
        internal static MongoClient Client;

        private static ILogger _logger;

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

        internal static void Activate()
        {
            CoreDispatcher.OnEventDispatched += OnEventDispatchedMethod;
            Client = new MongoClient(ServersUrls);
        }

        internal static void Deactivate()
        {
            CoreDispatcher.OnEventDispatched -= OnEventDispatchedMethod;
            Client = null;
        }

        internal static async Task OnEventDispatchedMethod(IDomainEvent @event)
        {
            try
            {
                await new MongoDbEventStore().StoreDomainEventAsync(@event).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                _logger?.LogError($"EventHandler.OnEventDispatchedMethod() : Exception {exc}");
            }
        }

        #endregion

    }
}
