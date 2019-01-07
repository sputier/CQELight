using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.Attributes;
using CQELight.EventStore.EFCore.Common;
using CQELight.EventStore.EFCore.Models;
using CQELight.Tools;
using CQELight.Tools.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.EventStore.EFCore
{
    internal class EFEventStore : DisposableObject, IEventStore, IAggregateEventStore
    {

        #region Static members

        private static System.Timers.Timer s_AbsoluteTimer;
        private static System.Timers.Timer s_SlidingTimer;
        private static SemaphoreSlim s_Lock = new SemaphoreSlim(1);
        private static ConcurrentBag<Event> s_Events = new ConcurrentBag<Event>();
        private static SemaphoreSlim s_TimerLock = new SemaphoreSlim(1);

        #endregion

        #region Members

        private readonly ILogger _logger;
        private readonly ISnapshotBehaviorProvider _snapshotBehaviorProvider;
        private readonly BufferInfo _bufferInfo;
        private readonly EventArchiveBehaviorInfos _archiveBehaviorInfos;
        private readonly DbContextOptions _dbContextOptions;

        #endregion

        #region Ctor

        public EFEventStore(DbContextOptions dbContextOptions, ILoggerFactory loggerFactory = null,
            ISnapshotBehaviorProvider snapshotBehaviorProvider = null, BufferInfo bufferInfo = null,
            EventArchiveBehaviorInfos archiveBehaviorInfos = null)
        {
            _dbContextOptions = dbContextOptions ?? throw new ArgumentNullException(nameof(dbContextOptions));
            _logger = (loggerFactory ?? new LoggerFactory()).CreateLogger<EFEventStore>();
            _snapshotBehaviorProvider = snapshotBehaviorProvider ?? EventStoreManager.SnapshotBehaviorProvider;

            _bufferInfo = bufferInfo;
            if (bufferInfo?.UseBuffer == true && s_AbsoluteTimer == null && s_SlidingTimer == null)
            {
                s_AbsoluteTimer = new System.Timers.Timer((int)bufferInfo.AbsoluteTimeOut.TotalMilliseconds)
                {
                    AutoReset = true
                };
                s_SlidingTimer = new System.Timers.Timer((int)bufferInfo.SlidingTimeOut.TotalMilliseconds)
                {
                    AutoReset = true
                };
            }

            _archiveBehaviorInfos = archiveBehaviorInfos;
        }

        #endregion

        #region IEventStore

        /// <summary>
        /// Get an event per its id.
        /// </summary>
        /// <param name="eventId">Id of the event.</param>
        /// <returns>Instance of the event.</returns>
        public async Task<TEvent> GetEventByIdAsync<TEvent>(Guid eventId)
            where TEvent : class, IDomainEvent
        {
            if (_bufferInfo?.UseBuffer == true)
            {
                await s_Lock.WaitAsync().ConfigureAwait(false);
            }
            try
            {
                using (var ctx = new EventStoreDbContext(_dbContextOptions,
                    _archiveBehaviorInfos?.ArchiveBehavior ?? SnapshotEventsArchiveBehavior.StoreToNewDatabase))
                {
                    var evt = await ctx.FindAsync<Event>(eventId).ConfigureAwait(false);
                    if (evt != null)
                    {
                        return GetRehydratedEventFromDbEvent(evt) as TEvent;
                    }
                    return null;
                }
            }
            finally
            {
                s_Lock.Release();
            }
        }

        /// <summary>
        /// Get a collection of events for a specific aggregate.
        /// </summary>
        /// <param name="aggregateUniqueId">Id of the aggregate which we want all the events.</param>
        /// <typeparam name="TAggregate">Aggregate type.</typeparam>
        /// <returns>Collection of all associated events.</returns>
        public Task<IAsyncEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TAggregate, TId>(TId aggregateUniqueId)
            where TAggregate : EventSourcedAggregate<TId>
            => GetEventsFromAggregateIdAsync<TId>(aggregateUniqueId, typeof(TAggregate));

        /// <summary>
        /// Get a collection of events for a specific aggregate.
        /// </summary>
        /// <param name="aggregateUniqueId">Id of the aggregate which we want all the events.</param>
        /// <typeparam name="TAggregate">Aggregate type.</typeparam>
        /// <returns>Collection of all associated events.</returns>
        public async Task<IAsyncEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TId>(TId aggregateUniqueId, Type aggregateType)
        {
            if (_bufferInfo?.UseBuffer == true)
            {
                await s_Lock.WaitAsync().ConfigureAwait(false);
            }
            try
            {
                using (var ctx = new EventStoreDbContext(_dbContextOptions,
                    _archiveBehaviorInfos?.ArchiveBehavior ?? SnapshotEventsArchiveBehavior.StoreToNewDatabase))
                {
                    var hashedId = aggregateUniqueId.ToJson(true).GetHashCode();
                    var events = await ctx
                    .Set<Event>()
                    .Where(e => e.HashedAggregateId == hashedId && e.AggregateType == aggregateType.AssemblyQualifiedName)
                    .ToListAsync().ConfigureAwait(false);

                    var result = new List<IDomainEvent>();
                    foreach (var evt in events)
                    {
                        try
                        {
                            result.Add(GetRehydratedEventFromDbEvent(evt));
                        }
                        catch (Exception e)
                        {
                            _logger.LogErrorMultilines("EFEventStore.GetEventsFromAggregateIdAsync() : An event has not been rehydrated correctly.",
                                e.ToString(), "Event data is : ", evt.EventData, "Event type is : ", evt.EventType);
                        }
                    }
                    return result.ToAsyncEnumerable();
                }
            }
            finally
            {
                s_Lock.Release();
            }
        }

        /// <summary>
        /// Store a domain event in the event store
        /// </summary>
        /// <param name="event">Event instance to be persisted.</param>
        public async Task StoreDomainEventAsync(IDomainEvent @event)
        {
            var evtType = @event.GetType();
            if (evtType.IsDefined(typeof(EventNotPersistedAttribute)))
            {
                return;
            }
            try
            {
                if (_bufferInfo?.UseBuffer == true)
                {
                    await s_Lock.WaitAsync().ConfigureAwait(false);
                }
                using (var ctx = new EventStoreDbContext(_dbContextOptions,
                    _archiveBehaviorInfos?.ArchiveBehavior ?? SnapshotEventsArchiveBehavior.StoreToNewDatabase))
                {
                    int sequence = Convert.ToInt32(@event.Sequence);
                    if (@event.AggregateId != null)
                    {
                        var hashedAggregateId = @event.AggregateId.ToJson(true).GetHashCode();
                        if (_snapshotBehaviorProvider != null)
                        {
                            var behavior = _snapshotBehaviorProvider.GetBehaviorForEventType(evtType);
                            if (behavior != null && await behavior.IsSnapshotNeededAsync(@event.AggregateId, @event.AggregateType)
                                .ConfigureAwait(false))
                            {
                                var rehydratedAggregate = await GetRehydratedAggregateAsync(@event.AggregateId, @event.AggregateType).ConfigureAwait(false);
                                var result = await behavior.GenerateSnapshotAsync(@event.AggregateId, @event.AggregateType, rehydratedAggregate)
                                    .ConfigureAwait(false);
                                if (result.Snapshot is Snapshot snapshot)
                                {
                                    var previousSnapshot = await ctx.Set<Snapshot>().FirstOrDefaultAsync(s => s.HashedAggregateId == hashedAggregateId
                                        && s.AggregateType == @event.AggregateType.AssemblyQualifiedName);
                                    if (previousSnapshot != null)
                                    {
                                        ctx.Remove(previousSnapshot);
                                    }
                                    ctx.Add(snapshot);
                                    sequence = result.NewSequence;
                                }
                                if (result.ArchiveEvents?.Any() == true)
                                {
                                    await StoreArchiveEventsAsync(result.ArchiveEvents).ConfigureAwait(false);
                                }
                            }
                        }
                        if (sequence == 0)
                        {
                            sequence = await ctx
                                .Set<Event>().CountAsync(t => t.HashedAggregateId == hashedAggregateId)
                                .ConfigureAwait(false);
                            sequence++;
                        }
                    }
                    var persistableEvent = GetEventFromIDomainEvent(@event, sequence);
                    if (_bufferInfo?.UseBuffer == true)
                    {
                        s_Events.Add(persistableEvent);
                        async void Elapsed(object o, System.Timers.ElapsedEventArgs e)
                        {
                            s_AbsoluteTimer.Elapsed -= Elapsed;
                            s_SlidingTimer.Elapsed -= Elapsed;
                            s_SlidingTimer.Stop();
                            s_AbsoluteTimer.Stop();
                            await s_TimerLock.WaitAsync().ConfigureAwait(false);
                            {
                                try
                                {
                                    if (s_Events.Count > 0)
                                    {
                                        using (var innerCtx = new EventStoreDbContext(_dbContextOptions,
                                            _archiveBehaviorInfos?.ArchiveBehavior ?? SnapshotEventsArchiveBehavior.StoreToNewDatabase))
                                        {
                                            innerCtx.AddRange(s_Events);
                                            s_Events = new ConcurrentBag<Event>();
                                            await ctx.SaveChangesAsync().ConfigureAwait(false);
                                        }
                                    }
                                }
                                finally
                                {
                                    s_TimerLock.Release();
                                }
                            }
                        }

                        if (!s_AbsoluteTimer.Enabled && !s_SlidingTimer.Enabled)
                        {
                            s_AbsoluteTimer.Elapsed += Elapsed;
                            s_SlidingTimer.Elapsed += Elapsed;
                            s_AbsoluteTimer.Start();
                            s_SlidingTimer.Start();
                        }
                    }
                    else
                    {
                        ctx.Add(persistableEvent);
                    }
                    await ctx.SaveChangesAsync().ConfigureAwait(false);
                }

            }
            finally
            {
                s_Lock.Release();
            }
        }

        #endregion

        #region IAggregateEventStore

        /// <summary>
        /// Retrieve a rehydrated aggregate from its unique Id and its type.
        /// </summary>
        /// <param name="aggregateUniqueId">Aggregate unique id.</param>
        /// <param name="aggregateType">Aggregate type.</param>
        /// <returns>Rehydrated event source aggregate.</returns>
        public async Task<IEventSourcedAggregate> GetRehydratedAggregateAsync<TId>(TId aggregateUniqueId, Type aggregateType)
        {
            if (aggregateType == null)
            {
                throw new ArgumentNullException(nameof(aggregateType));
            }

            if (!(aggregateType.CreateInstance() is IEventSourcedAggregate aggInstance))
            {
                throw new InvalidOperationException("EFEventStore.GetRehydratedAggregateAsync() : Cannot create a new instance of" +
                    $" {aggregateType.FullName} aggregate. It should have one parameterless constructor (can be private).");
            }

            var events = await (await GetEventsFromAggregateIdAsync(aggregateUniqueId, aggregateType).ConfigureAwait(false)).ToList().ConfigureAwait(false);
            Snapshot snapshot = null;
            using (var ctx = new EventStoreDbContext(_dbContextOptions,
                    _archiveBehaviorInfos?.ArchiveBehavior ?? SnapshotEventsArchiveBehavior.StoreToNewDatabase))
            {
                var hashedAggregateId = aggregateUniqueId.ToJson(true).GetHashCode();
                snapshot = await ctx.Set<Snapshot>()
                    .Where(t => t.AggregateType == aggregateType.AssemblyQualifiedName && t.HashedAggregateId == hashedAggregateId)
                    .FirstOrDefaultAsync().ConfigureAwait(false);
            }
            PropertyInfo stateProp = aggregateType.GetAllProperties().FirstOrDefault(p => p.PropertyType.IsSubclassOf(typeof(AggregateState)));
            FieldInfo stateField = aggregateType.GetAllFields().FirstOrDefault(f => f.FieldType.IsSubclassOf(typeof(AggregateState)));
            Type stateType = stateProp?.PropertyType ?? stateField?.FieldType;
            if (stateType != null)
            {
                object state = null;
                if (snapshot != null)
                {
                    state = snapshot.SnapshotData.FromJson(stateType);
                }
                else
                {
                    state = stateType.CreateInstance();
                }

                if (stateProp != null)
                {
                    stateProp.SetValue(aggInstance, state);
                }
                else
                {
                    stateField.SetValue(aggInstance, state);
                }
            }
            else
            {
                throw new InvalidOperationException("EFEventStore.GetRehydratedAggregateAsync() : Cannot find property/field that manage state for aggregate" +
                    $" type {aggregateType.FullName}. State should be a property or a field of the aggregate");
            }
            aggInstance.RehydrateState(events);

            return aggInstance;
        }

        /// <summary>
        /// Retrieve a rehydrated aggregate from its unique Id and its type.
        /// </summary>
        /// <param name="aggregateUniqueId">Aggregate unique id.</param>
        /// <returns>Rehydrated event source aggregate.</returns>
        /// <typeparam name="T">Type of aggregate to retrieve</typeparam>
        public async Task<TAggregate> GetRehydratedAggregateAsync<TAggregate, TId>(TId aggregateUniqueId) where TAggregate : EventSourcedAggregate<TId>, new()
            => (await GetRehydratedAggregateAsync(aggregateUniqueId, typeof(TAggregate)).ConfigureAwait(false)) as TAggregate;

        #endregion

        #region Overriden methods

        protected override void Dispose(bool disposing)
        {
        }

        #endregion

        #region Private methods

        private async Task StoreArchiveEventsAsync(IEnumerable<IDomainEvent> archiveEvents)
        {
            switch (_archiveBehaviorInfos?.ArchiveBehavior)
            {
                case SnapshotEventsArchiveBehavior.StoreToNewTable:
                    using (var ctx = new ArchiveEventStoreDbContext(_dbContextOptions))
                    {
                        ctx.AddRange(archiveEvents.Select(GetArchiveEventFromIDomainEvent).ToList());
                        await ctx.SaveChangesAsync().ConfigureAwait(false);
                    }
                    break;
                case SnapshotEventsArchiveBehavior.StoreToNewDatabase:
                    using (var ctx = new ArchiveEventStoreDbContext(_archiveBehaviorInfos.ArchiveDbContextOptions))
                    {
                        ctx.AddRange(
                            archiveEvents.Select(GetArchiveEventFromIDomainEvent));
                        await ctx.SaveChangesAsync().ConfigureAwait(false);
                    }
                    break;
            }
            using (var ctx = new EventStoreDbContext(_dbContextOptions))
            {
                var events = await ctx.Set<Event>().Where(e => archiveEvents.Any(ev => ev.Id == e.Id)).ToListAsync().ConfigureAwait(false);
                ctx.RemoveRange(events);
                await ctx.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private IDomainEvent GetRehydratedEventFromDbEvent(Event evt)
        {
            var evtType = Type.GetType(evt.EventType);
            var rehydratedEvt = evt.EventData.FromJson(evtType) as IDomainEvent;
            var properties = evtType.GetAllProperties();

            if (!string.IsNullOrWhiteSpace(evt.SerializedAggregateId) && !string.IsNullOrWhiteSpace(evt.AggregateIdType))
            {
                var aggregateIdType = Type.GetType(evt.AggregateIdType);
                if (aggregateIdType != null)
                {
                    properties.First(p => p.Name == nameof(IDomainEvent.AggregateId)).SetMethod?
                        .Invoke(rehydratedEvt, new object[] { Encoding.UTF8.GetString(Convert.FromBase64String(evt.SerializedAggregateId))
                        .FromJson(aggregateIdType) });
                }
            }
            properties.First(p => p.Name == nameof(IDomainEvent.Id)).SetMethod?.Invoke(rehydratedEvt, new object[] { evt.Id });
            properties.First(p => p.Name == nameof(IDomainEvent.EventTime)).SetMethod?.Invoke(rehydratedEvt, new object[] { evt.EventTime });
            properties.First(p => p.Name == nameof(IDomainEvent.Sequence)).SetMethod?.Invoke(rehydratedEvt, new object[] { Convert.ToUInt64(evt.Sequence) });
            return rehydratedEvt;
        }

        private ArchiveEvent GetArchiveEventFromIDomainEvent(IDomainEvent @event)
        {
            var jsonId = @event.AggregateId != null ? @event.AggregateId.ToJson(true) : string.Empty;
            return new ArchiveEvent
            {
                Id = @event.Id != Guid.Empty ? @event.Id : Guid.NewGuid(),
                EventData = @event.ToJson(),
                HashedAggregateId = !string.IsNullOrWhiteSpace(jsonId) ? jsonId.GetHashCode() : new int?(),
                SerializedAggregateId = !string.IsNullOrWhiteSpace(jsonId) ? Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonId)) : string.Empty,
                AggregateIdType = @event.AggregateId?.GetType()?.AssemblyQualifiedName,
                AggregateType = @event.AggregateType?.AssemblyQualifiedName,
                EventType = @event.GetType().AssemblyQualifiedName,
                EventTime = @event.EventTime,
                Sequence = (long)@event.Sequence
            };
        }
        private Event GetEventFromIDomainEvent(IDomainEvent @event, long currentSeq)
        {
            var jsonId = @event.AggregateId != null ? @event.AggregateId.ToJson(true) : string.Empty;
            return new Event
            {
                Id = @event.Id != Guid.Empty ? @event.Id : Guid.NewGuid(),
                EventData = @event.ToJson(),
                HashedAggregateId = !string.IsNullOrWhiteSpace(jsonId) ? jsonId.GetHashCode() : new int?(),
                SerializedAggregateId = !string.IsNullOrWhiteSpace(jsonId) ? Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonId)) : string.Empty,
                AggregateIdType = @event.AggregateId?.GetType()?.AssemblyQualifiedName,
                AggregateType = @event.AggregateType?.AssemblyQualifiedName,
                EventType = @event.GetType().AssemblyQualifiedName,
                EventTime = @event.EventTime,
                Sequence = currentSeq
            };
        }

        #endregion

    }
}
