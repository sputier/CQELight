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
    internal class EFEventStore : IEventStore, IAggregateEventStore
    {

        #region Static members
        
        private static DateTime? s_BufferEnteredTimeAbsolute;
        private static DateTime? s_BufferEnteredTimeSliding;
        private static Timer s_AbsoluteTimer;
        private static Timer s_SlidingTimer;

        private static SemaphoreSlim s_Lock = new SemaphoreSlim(2);
        private static ConcurrentBag<Event> s_Events = new ConcurrentBag<Event>();

        #endregion

        #region Members

        private readonly ILogger _logger;
        private readonly ISnapshotBehaviorProvider _snapshotBehaviorProvider;
        private readonly BufferInfo _bufferInfo;
        private readonly EventArchiveBehaviorInfos _archiveBehaviorInfos;
        private readonly DbContextOptions<EventStoreDbContext> _dbContextOptions;

        #endregion

        #region Ctor

        public EFEventStore(
            DbContextOptions<EventStoreDbContext> dbContextOptions,
            ILoggerFactory loggerFactory = null,
            ISnapshotBehaviorProvider snapshotBehaviorProvider = null,
            BufferInfo bufferInfo = null,
            EventArchiveBehaviorInfos archiveBehaviorInfos = null)
        {
            _dbContextOptions = dbContextOptions ?? throw new ArgumentNullException(nameof(dbContextOptions));
            _logger = (loggerFactory ?? new LoggerFactory()).CreateLogger<EFEventStore>();
            _snapshotBehaviorProvider = snapshotBehaviorProvider ?? EventStoreManager.SnapshotBehaviorProvider;

            _bufferInfo = bufferInfo;
            _archiveBehaviorInfos = archiveBehaviorInfos;

            if (_bufferInfo != null)
            {
                s_SlidingTimer = new Timer(TreatBufferEvents, null, Timeout.Infinite, Timeout.Infinite);
                s_AbsoluteTimer = new Timer(TreatBufferEvents, null, Timeout.Infinite, Timeout.Infinite);
            }
        }

        #endregion

        #region IEventStore
        
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
        
        public Task<IAsyncEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TAggregate, TId>(TId aggregateUniqueId)
            where TAggregate : EventSourcedAggregate<TId>
            => GetEventsFromAggregateIdAsync<TId>(aggregateUniqueId, typeof(TAggregate));
        
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
                    _archiveBehaviorInfos?.ArchiveBehavior ?? SnapshotEventsArchiveBehavior.Disabled))
                {
                    var sequence = Convert.ToInt64(@event.Sequence);                    
                    if (@event.AggregateId != null)
                    {
                        var hashedAggregateId = @event.AggregateId.ToJson(true).GetHashCode();
                        if (sequence == 0)
                        {
                            if (_bufferInfo?.UseBuffer == true)
                            {
                                sequence = s_Events.Max(e => (long?)e.Sequence) ?? 0;
                            }
                            if (sequence == 0)
                            {
                                sequence = await ctx
                                    .Set<Event>()
                                    .AsNoTracking()
                                    .Where(t => t.HashedAggregateId == hashedAggregateId)
                                    .MaxAsync(e => (long?)e.Sequence)
                                    .ConfigureAwait(false) ?? 0;
                            }
                            sequence++;
                        }
                        await ManageSnapshotBehavior(@event, ctx, hashedAggregateId).ConfigureAwait(false);
                        
                    }
                    var persistableEvent = GetEventFromIDomainEvent(@event, sequence);
                    if (_bufferInfo?.UseBuffer == true)
                    {
                        s_Events.Add(persistableEvent);

                        if (!s_BufferEnteredTimeAbsolute.HasValue && !s_BufferEnteredTimeSliding.HasValue)
                        {
                            s_BufferEnteredTimeAbsolute = DateTime.Now;
                            s_BufferEnteredTimeSliding = DateTime.Now;
                            s_AbsoluteTimer.Change(Convert.ToInt32(_bufferInfo.AbsoluteTimeOut.TotalMilliseconds), Timeout.Infinite);
                            s_SlidingTimer.Change(Convert.ToInt32(_bufferInfo.SlidingTimeOut.TotalMilliseconds), Timeout.Infinite);
                        }
                        else
                        {
                            if (DateTime.Now.Subtract(s_BufferEnteredTimeAbsolute.Value).TotalMilliseconds >= _bufferInfo.AbsoluteTimeOut.TotalMilliseconds)
                            {
                                TreatBufferEvents(null);
                                s_BufferEnteredTimeAbsolute = null;
                                s_BufferEnteredTimeSliding = null;
                            }
                            else if (DateTime.Now.Subtract(s_BufferEnteredTimeAbsolute.Value).TotalMilliseconds >= _bufferInfo.SlidingTimeOut.TotalMilliseconds)
                            {
                                TreatBufferEvents(null);
                                s_BufferEnteredTimeAbsolute = null;
                                s_BufferEnteredTimeSliding = null;
                            }
                            else
                            {
                                s_BufferEnteredTimeSliding = DateTime.Now;
                                s_SlidingTimer.Change(Convert.ToInt32(_bufferInfo.SlidingTimeOut.TotalMilliseconds), Timeout.Infinite);
                            }
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
        
        public async Task<TAggregate> GetRehydratedAggregateAsync<TAggregate, TId>(TId aggregateUniqueId)
            where TAggregate : EventSourcedAggregate<TId>, new()
            => (await GetRehydratedAggregateAsync(aggregateUniqueId, typeof(TAggregate)).ConfigureAwait(false)) as TAggregate;

        #endregion

        #region Private methods

        private async Task ManageSnapshotBehavior(
            IDomainEvent @event,
            EventStoreDbContext ctx,
            int hashedAggregateId)
        {

            bool IsSnaphostEnabled()
            {
                return _snapshotBehaviorProvider != null
                        && _archiveBehaviorInfos != null
                        && _archiveBehaviorInfos.ArchiveBehavior != SnapshotEventsArchiveBehavior.Disabled;
            }
            
            var evtType = @event.GetType();
            if (IsSnaphostEnabled())
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
                        var previousSnapshot = await ctx
                            .Set<Snapshot>()
                            .FirstOrDefaultAsync(s =>
                                s.HashedAggregateId == hashedAggregateId &&
                                s.AggregateType == @event.AggregateType.AssemblyQualifiedName)
                            .ConfigureAwait(false);
                        if (previousSnapshot != null)
                        {
                            ctx.Remove(previousSnapshot);
                        }
                        ctx.Add(snapshot);
                    }
                    if (result.ArchiveEvents?.Any() == true)
                    {
                        await StoreArchiveEventsAsync(result.ArchiveEvents).ConfigureAwait(false);
                    }
                }
            }
            
        }

        private async Task StoreArchiveEventsAsync(IEnumerable<IDomainEvent> archiveEvents)
        {
            switch (_archiveBehaviorInfos?.ArchiveBehavior)
            {
                case SnapshotEventsArchiveBehavior.StoreToNewTable:
                    using (var ctx = new EventStoreDbContext(_dbContextOptions))
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
        async void TreatBufferEvents(object state)
        {
            await s_Lock.WaitAsync().ConfigureAwait(false);
            {
                try
                {
                    if (s_Events.Count > 0)
                    {
                        using (var innerCtx = new EventStoreDbContext(_dbContextOptions,
                            _archiveBehaviorInfos?.ArchiveBehavior ?? SnapshotEventsArchiveBehavior.Disabled))
                        {
                            innerCtx.AddRange(s_Events);
                            s_Events = new ConcurrentBag<Event>();
                            await innerCtx.SaveChangesAsync().ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    s_BufferEnteredTimeAbsolute = null;
                    s_BufferEnteredTimeSliding = null;
                    s_AbsoluteTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    s_SlidingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    s_Lock.Release();
                }
            }
        }

        private IDomainEvent GetRehydratedEventFromDbEvent(Event evt)
        {
            var evtType = Type.GetType(evt.EventType);
            var rehydratedEvt = evt.EventData.FromJson(evtType) as IDomainEvent;
            var properties = evtType.GetAllProperties();

            object eventAggregateId = null;
            if (!string.IsNullOrWhiteSpace(evt.SerializedAggregateId) && !string.IsNullOrWhiteSpace(evt.AggregateIdType))
            {
                var aggregateIdType = Type.GetType(evt.AggregateIdType);
                if (aggregateIdType != null)
                {
                    eventAggregateId = Encoding.UTF8.GetString(
                            Convert.FromBase64String(evt.SerializedAggregateId)).FromJson(aggregateIdType
                        );
                }
            }
            if (rehydratedEvt is BaseDomainEvent baseDomainEvent)
            {
                baseDomainEvent.Id = evt.Id;
                baseDomainEvent.EventTime = evt.EventTime;
                baseDomainEvent.Sequence = Convert.ToUInt64(evt.Sequence);
                baseDomainEvent.AggregateId = eventAggregateId;
            }
            else
            {
                properties.First(p => p.Name == nameof(IDomainEvent.Id)).SetMethod?.Invoke(rehydratedEvt, new object[] { evt.Id });
                properties.First(p => p.Name == nameof(IDomainEvent.EventTime)).SetMethod?.Invoke(rehydratedEvt, new object[] { evt.EventTime });
                properties.First(p => p.Name == nameof(IDomainEvent.Sequence)).SetMethod?.Invoke(rehydratedEvt, new object[] { Convert.ToUInt64(evt.Sequence) });
                properties.First(p => p.Name == nameof(IDomainEvent.AggregateId)).SetMethod?.Invoke(rehydratedEvt, new object[] { eventAggregateId });
            }
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
