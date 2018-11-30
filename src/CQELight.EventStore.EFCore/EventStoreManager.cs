using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Dispatcher;
using CQELight.EventStore.EFCore.Common;
using CQELight.IoC;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.EventStore.EFCore
{
    internal static class EventStoreManager
    {
        #region Internal static properties

        internal static DbContextOptions DbContextOptions { get; set; }
        internal static ISnapshotBehaviorProvider SnapshotBehaviorProvider { get; set; }
        internal static BufferInfo BufferInfo { get; set; } = BufferInfo.Disabled;

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

        internal static void Activate()
        {
            CoreDispatcher.OnEventDispatched += OnEventDispatchedMethod;

            //Because of EFCore way of genering migration, which is not database agnostic, we're forced to use the EnsureCreated method, which is incompatible
            //with model update. If model should be updated in next future, we will have to handle migration by ourselves, unless EF Core provides a migraton
            //which is database agnostic

            using (var ctx = new EventStoreDbContext(DbContextOptions))
            {
                ctx.Database.EnsureCreated();
            }
        }

        internal static void Deactivate()
        {
            CoreDispatcher.OnEventDispatched -= OnEventDispatchedMethod;
        }

        internal static async Task OnEventDispatchedMethod(IDomainEvent @event)
        {
            try
            {
                using (var store = new EFEventStore(new EventStoreDbContext(DbContextOptions)))
                {
                    await store.StoreDomainEventAsync(@event).ConfigureAwait(false);
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
