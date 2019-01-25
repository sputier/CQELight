using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Dispatcher;
using CQELight.EventStore.EFCore.Common;
using CQELight.IoC;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
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

        internal static DbContextOptions<EventStoreDbContext> DbContextOptions { get; set; }
        internal static ISnapshotBehaviorProvider SnapshotBehaviorProvider { get; set; }
        internal static BufferInfo BufferInfo { get; set; } = BufferInfo.Disabled;
        internal static EventArchiveBehaviorInfos ArchiveBehaviorInfos { get; set; }

        private static readonly ILogger _logger;
        private static readonly ILoggerFactory _loggerFactory;

        #endregion

        #region Static accessor
        static EventStoreManager()
        {
            if (DIManager.IsInit)
            {
                _loggerFactory = DIManager.BeginScope().Resolve<ILoggerFactory>();
                _logger = _loggerFactory?.CreateLogger("EventStore");
            }
            else
            {
                _loggerFactory = new LoggerFactory();
                _loggerFactory.AddProvider(new DebugLoggerProvider());
                _logger = _loggerFactory.CreateLogger(nameof(EventStoreManager));
            }
        }

        #endregion

        #region Public static methods

        internal static void Activate()
        {
            CoreDispatcher.OnEventDispatched += OnEventDispatchedMethod;

            using (var ctx = new EventStoreDbContext(DbContextOptions,
                    ArchiveBehaviorInfos?.ArchiveBehavior ?? SnapshotEventsArchiveBehavior.StoreToNewDatabase))
            {
                ctx.Database.Migrate();
            }
            if (ArchiveBehaviorInfos != null && ArchiveBehaviorInfos.ArchiveBehavior == SnapshotEventsArchiveBehavior.StoreToNewDatabase)
            {
                using (var ctx = new ArchiveEventStoreDbContext(ArchiveBehaviorInfos.ArchiveDbContextOptions))
                {
                    ctx.Database.Migrate();
                }
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
                using (var store = new EFEventStore(DbContextOptions,
                    _loggerFactory, SnapshotBehaviorProvider,
                    BufferInfo, ArchiveBehaviorInfos))
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
