using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.EFCore.Common;
using CQELight.EventStore.EFCore.Models;
using CQELight.Extensions;
using CQELight.Tools.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.EventStore.EFCore.Snapshots
{
    /// <summary>
    /// Default numeric snapshot behavior, that create a snapshot based of 
    /// the defined number of events for a specifc aggregate type, without
    /// any business logic.
    /// </summary>
    public class NumericSnapshotBehavior : ISnapshotBehavior
    {

        #region Members

        private readonly int _nbEvents;
        private readonly DbContextOptions<EventStoreDbContext> _configuration;

        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new NumericSnapshotBehavior, with specified value.
        /// </summary>
        /// <param name="nbEvents">Number of events before creating a snapshot.</param>
        /// <param name="configuration">Configuration for the DbContext of snapshot persistence. If not provided,
        /// same database than events will be used.</param>
        public NumericSnapshotBehavior(int nbEvents, DbContextOptions<EventStoreDbContext> dbContextOptions = null)
        {
            if (nbEvents < 2)
            {
                throw new ArgumentException("NumericSnapshotBehavior.ctor() : The number of events to generate" +
                    " a snapshot should be greater or equal to 2.");
            }
            _nbEvents = nbEvents;
            _configuration = dbContextOptions ?? EventStoreManager.DbContextOptions;
        }

        #endregion

        #region ISnapshotBehavior methods

        public async Task<(ISnapshot, IEnumerable<IDomainEvent>)> GenerateSnapshotAsync(object aggregateId, Type aggregateType,
            IEventSourcedAggregate rehydratedAggregate)
        {
            Snapshot snap = null;
            var archiveEventList = new List<IDomainEvent>();
            var hashedAggregateId = aggregateId.ToJson(true).GetHashCode();
            using (var ctx = new EventStoreDbContext(_configuration))
            {
                var orderedEvents =
                    await ctx.Set<Event>().Where(e => e.AggregateType == aggregateType.AssemblyQualifiedName
                    && e.HashedAggregateId == hashedAggregateId).OrderBy(e => e.Sequence).Take(_nbEvents).ToListAsync()
                    .ConfigureAwait(false);

                archiveEventList = orderedEvents.Select(d =>
                    d.EventData.FromJson(Type.GetType(d.EventType)) as IDomainEvent).ToList();

                snap = new Snapshot
                {
                    HashedAggregateId = hashedAggregateId,
                    AggregateType = aggregateType.AssemblyQualifiedName,
                    SnapshotBehaviorType = typeof(NumericSnapshotBehavior).AssemblyQualifiedName,
                    SnapshotTime = DateTime.Now,
                    SnapshotData = rehydratedAggregate.GetSerializedState()
                };

                await ctx.SaveChangesAsync().ConfigureAwait(false);
            }

            return (snap, archiveEventList);
        }

        public async Task<bool> IsSnapshotNeededAsync(object aggregateId, Type aggregateType)
        {
            using (var ctx = new EventStoreDbContext(_configuration))
            {
                var hashedAggregateId = aggregateId.ToJson(true).GetHashCode();
                return await ctx.Set<Event>().Where(e => e.AggregateType == aggregateType.AssemblyQualifiedName
                && e.HashedAggregateId == hashedAggregateId).CountAsync().ConfigureAwait(false) >= _nbEvents;
            }
        }

        #endregion

    }
}
