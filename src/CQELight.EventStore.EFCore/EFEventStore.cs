using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.EventStore.EFCore
{
    class EFEventStore : DisposableObject, IEventStore
    {

        #region Members

        private readonly EventStoreDbContext _dbContext;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public EFEventStore(EventStoreDbContext dbContext, ILoggerFactory loggerFactory = null)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = (loggerFactory ?? new LoggerFactory()).CreateLogger<EFEventStore>();
        }

        #endregion

        #region IEventStore

        /// <summary>
        /// Get an event per its id.
        /// </summary>
        /// <param name="eventId">Id of the event.</param>
        /// <returns>Instance of the event.</returns>
        public async Task<TEvent> GetEventById<TEvent>(Guid eventId)
            where TEvent : class, IDomainEvent
        {
            var evt = await _dbContext.FindAsync<Event>(eventId);
            if (evt != null)
            {
                return GetRehydratedEventFromDbEvent(evt) as TEvent;

            }
            return null;
        }

        /// <summary>
        /// Get a collection of events for a specific aggregate.
        /// </summary>
        /// <param name="aggregateUniqueId">Id of the aggregate which we want all the events.</param>
        /// <typeparam name="TAggregate">Aggregate type.</typeparam>
        /// <returns>Collection of all associated events.</returns>
        public async Task<IEnumerable<IDomainEvent>> GetEventsFromAggregateIdAsync<TAggregate>(Guid aggregateUniqueId)
        {
            var events = await _dbContext.Set<Event>()
                .Where(e => e.AggregateId == aggregateUniqueId && e.AggregateType == typeof(TAggregate).AssemblyQualifiedName).ToListAsync();

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
            return result.AsEnumerable();
        }


        /// <summary>
        /// Store a domain event in the event store
        /// </summary>
        /// <param name="event">Event instance to be persisted.</param>
        public async Task StoreDomainEventAsync(IDomainEvent @event)
        {
            if (@event.GetType().IsDefined(typeof(EventNotPersistedAttribute)))
            {
                return;
            }
            var currentSeq = await _dbContext.Set<Event>().CountAsync(t => t.AggregateId == @event.AggregateId);
            PersistSingleEvent(@event, ++currentSeq);
            await _dbContext.SaveChangesAsync();
        }

        #endregion

        #region Overriden methods

        protected override void Dispose(bool disposing)
        {
            try
            {
                _dbContext.Dispose();
            }
            catch
            {
                // No need to log for this
            }
        }

        #endregion

        #region Private methods

        private IDomainEvent GetRehydratedEventFromDbEvent(Event evt)
        {
            var evtType = Type.GetType(evt.EventType);
            var rehydratedEvt = evt.EventData.FromJson(evtType) as IDomainEvent;
            var properties = evtType.GetAllProperties();

            properties.First(p => p.Name == nameof(IDomainEvent.AggregateId)).SetMethod?.Invoke(rehydratedEvt, new object[] { evt.AggregateId });
            properties.First(p => p.Name == nameof(IDomainEvent.Id)).SetMethod?.Invoke(rehydratedEvt, new object[] { evt.Id });
            properties.First(p => p.Name == nameof(IDomainEvent.EventTime)).SetMethod?.Invoke(rehydratedEvt, new object[] { evt.EventTime });
            properties.First(p => p.Name == nameof(IDomainEvent.Sequence)).SetMethod?.Invoke(rehydratedEvt, new object[] { Convert.ToUInt64(evt.Sequence) });
            return rehydratedEvt;
        }

        private void PersistSingleEvent(IDomainEvent @event, long currentSeq)
        {
            var persitableEvent = new Event
            {
                Id = @event.Id,
                EventData = @event.ToJson(),
                AggregateId = @event.AggregateId,
                AggregateType = @event.AggregateType?.AssemblyQualifiedName,
                EventType = @event.GetType().AssemblyQualifiedName,
                EventTime = @event.EventTime,
                Sequence = currentSeq
            };
            _dbContext.Add(persitableEvent);
        }

        #endregion

    }
}
