using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Dispatcher;
using CQELight.EventStore.MongoDb.Common;
using CQELight.IoC;
using CQELight.Tools.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.EventStore.MongoDb
{
    internal static class EventStoreManager
    {

        #region Static members

        private static MongoClient _client;

        #endregion

        #region Internal static properties

        internal static MongoEventStoreOptions Options;

        internal static MongoClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new MongoClient(ExtractUrlFromOptions());
                }
                return _client;
            }
            set
            {
                _client = value;
            }
        }

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
                var loggerFactory = new LoggerFactory();
                loggerFactory.AddProvider(new DebugLoggerProvider());
                _logger = loggerFactory.CreateLogger(nameof(EventStoreManager));
            }
        }

        #endregion

        #region Private static methods

        private static MongoUrl ExtractUrlFromOptions()
        {
            var urlBuilder = new MongoUrlBuilder
            {
                Servers = Options.ServerUrls.Select(u => new MongoServerAddress(u)),
                Username = Options.Username,
                Password = Options.Password
            };
            return urlBuilder.ToMongoUrl();
        }

        #endregion

        #region Internal static methods

        internal static void Activate()
        {
            CoreDispatcher.OnEventDispatched += OnEventDispatchedMethod;
            Client = new MongoClient(ExtractUrlFromOptions());
        }

        internal static void Deactivate()
        {
            CoreDispatcher.OnEventDispatched -= OnEventDispatchedMethod;
            Client = null;
        }

        internal static async Task OnEventDispatchedMethod(IDomainEvent @event)
        {
            if (Client != null)
            {
                try
                {
                    await new MongoDbEventStore(Options.SnapshotBehaviorProvider).StoreDomainEventAsync(@event).ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    _logger?.LogError($"EventHandler.OnEventDispatchedMethod() : Exception {exc}");
                }
            }
        }

        #endregion

    }
}
