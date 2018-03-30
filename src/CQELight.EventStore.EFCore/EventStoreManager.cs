using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Dispatcher;
using CQELight.EventStore.EFCore.Common;
using CQELight.IoC;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.EventStore.EFCore
{
    static class EventStoreManager
    {

        #region Internal static properties

        internal static DbContextConfiguration DbContextConfiguration { get; set; }

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
                if (DIManager.IsInit)
                {
                    var store = DIManager.BeginScope().Resolve<IEventStore>();
                    await store.StoreDomainEventAsync(@event);
                }
                else
                {
                    using (var store = new EFEventStore(new EventStoreDbContext(DbContextConfiguration)))
                    {
                        await store.StoreDomainEventAsync(@event);
                    }
                }
            }
            catch (Exception exc)
            {
                DIManager.BeginScope().Resolve<ILoggerFactory>().CreateLogger("EventStore")
                    .LogError($"EventHandler.OnEventDispatchedMethod() : Exception {exc}");
            }
        }

        #endregion


    }
}
