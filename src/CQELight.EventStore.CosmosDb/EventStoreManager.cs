using CQELight.Abstractions.Events.Interfaces;
using CQELight.Dispatcher;
using CQELight.EventStore.CosmosDb.Models;
using CQELight.IoC;
using CQELight.Tools.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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

        internal static IDomainEvent GetRehydratedEventFromDbEvent(Event evt)
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt));
            }

            var evtType = Type.GetType(evt.EventType);
            var rehydratedEvt = evt.EventData.FromJson(evtType) as IDomainEvent;
            var properties = evtType.GetAllProperties();

            properties.FirstOrDefault(p => p.Name == nameof(IDomainEvent.AggregateId))?
                .SetMethod?.Invoke(rehydratedEvt, new object[] { evt.AggregateId });
            properties.FirstOrDefault(p => p.Name == nameof(IDomainEvent.Id))?
                .SetMethod?.Invoke(rehydratedEvt, new object[] { evt.Id });
            properties.FirstOrDefault(p => p.Name == nameof(IDomainEvent.EventTime))?
                .SetMethod?.Invoke(rehydratedEvt, new object[] { evt.EventTime });
            properties.FirstOrDefault(p => p.Name == nameof(IDomainEvent.Sequence))?
                .SetMethod?.Invoke(rehydratedEvt, new object[] { Convert.ToUInt64(evt.Sequence) });
            return rehydratedEvt;
        }

        #endregion

    }
}
