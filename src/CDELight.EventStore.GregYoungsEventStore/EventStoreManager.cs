using CDELight.EventStore.GregYoungsEventStore;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Dispatcher;
using CQELight.IoC;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace CQELight.EventStore.GregYoungsEventStore
{
    internal static class EventStoreManager
    {
        private static IEventStoreConnection _eventStoreConnection;

        internal static GregYoungsEventStoreConfiguration Configuration;
        internal static IEventStoreConnection Connection
        {
            get
            {
                if(_eventStoreConnection == null)
                {
                    var connectionSettings = ConnectionSettings.Create();
                    if(Configuration != null)
                    {
                        if (!string.IsNullOrEmpty(Configuration.CredentialsUserName) && !string.IsNullOrEmpty(Configuration.CredentialsUserPassword))
                        {
                            connectionSettings.SetDefaultUserCredentials(new UserCredentials(Configuration.CredentialsUserName, Configuration.CredentialsUserPassword));
                        }
                        if (!string.IsNullOrEmpty(Configuration.SslConnectionTargetHost))
                        {
                            connectionSettings.UseSslConnection(Configuration.SslConnectionTargetHost, Configuration.SslConnectionValidateServer);
                        }
                        _eventStoreConnection = EventStoreConnection.Create(connectionSettings: connectionSettings, uri: Configuration.Uri);
                    }
                }
                return _eventStoreConnection;
            }
            set
            {
                _eventStoreConnection = value;
            }
        }
        internal static ISnapshotBehaviorProvider SnapshotBehaviorProvider;
        private static readonly Microsoft.Extensions.Logging.ILogger _logger;

        #region Static accessor
        static EventStoreManager()
        {
            if (DIManager.IsInit)
                _logger = DIManager.BeginScope().Resolve<ILoggerFactory>()?.CreateLogger("EventStore");
            else
                _logger = new LoggerFactory().AddDebug().CreateLogger(nameof(EventStoreManager));
        }

        #endregion

        #region Public static methods

        internal static void Activate()
        {
            CoreDispatcher.OnEventDispatched += OnEventDispatchedMethod;
        }

        internal static void Deactivate()
        {
            CoreDispatcher.OnEventDispatched -= OnEventDispatchedMethod;
        }

        internal static async Task OnEventDispatchedMethod(IDomainEvent @event)
        {
            try
            {
                await new GYEventStore(SnapshotBehaviorProvider).StoreDomainEventAsync(@event).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                _logger?.LogError($"EventHandler.OnEventDispatchedMethod() : Exception {exc}");
            }
        }

        #endregion
    }
}
