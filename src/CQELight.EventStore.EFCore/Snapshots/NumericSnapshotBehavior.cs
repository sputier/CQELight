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
        private readonly DbContextOptions _configuration;

        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new NumericSnapshotBehavior, with specified value.
        /// </summary>
        /// <param name="nbEvents">Number of events before creating a snapshot.</param>
        /// <param name="configuration">Configuration for the DbContext of snapshot persistence. If not provided,
        /// same database than events will be used.</param>
        public NumericSnapshotBehavior(int nbEvents, DbContextOptions dbContextOptions = null)
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

        /// <summary>
        /// Generate a new snapshot based on the aggregate id and the aggregate type.
        /// </summary>
        /// <param name="aggregateId">Id of the aggregate.</param>
        /// <param name="aggregateType">Type of the aggregate.</param>
        /// <returns>A new snapshot instance, the new sequence for next events and the collection of events to archive.</returns>
        public async Task<(ISnapshot, int, IEnumerable<IDomainEvent>)> GenerateSnapshotAsync(Guid aggregateId, Type aggregateType,
            ISnapshot previousSnapshot = null)
        {
            Snapshot snap = null;
            int newSequence = 1;
            var aggregateInstance = aggregateType.CreateInstance() as IEventSourcedAggregate;
            var archiveEventList = new List<IDomainEvent>();
            if (aggregateInstance != null)
            {
                using (var ctx = new EventStoreDbContext(_configuration))
                {
                    var orderedEvents =
                        await ctx.Set<Event>().Where(e => e.AggregateType == aggregateType.AssemblyQualifiedName
                        && e.AggregateId == aggregateId).OrderBy(e => e.Sequence).Take(_nbEvents).ToListAsync()
                        .ConfigureAwait(false);

                    archiveEventList = orderedEvents.Select(d =>
                        d.EventData.FromJson(Type.GetType(d.EventType)) as IDomainEvent).ToList();
                    aggregateInstance.RehydrateState(archiveEventList);

                    snap = new Snapshot
                    {
                        AggregateId = aggregateId,
                        AggregateType = aggregateType.AssemblyQualifiedName,
                        SnapshotBehaviorType = typeof(NumericSnapshotBehavior).AssemblyQualifiedName,
                        SnapshotTime = DateTime.Now,
                        SnapshotData = aggregateInstance.GetSerializedState()
                    };

                    //TODO store events into archive database instead of just removing them
                    ctx.RemoveRange(orderedEvents);
                    await ctx.SaveChangesAsync().ConfigureAwait(false);
                }

            }
            return (snap, newSequence, archiveEventList);
        }

        /// <summary>
        /// Get the info if a snapshot is needed, based on the aggregate id and the aggregate type.
        /// </summary>
        /// <param name="aggregateId">Id of the aggregate.</param>
        /// <param name="aggregateType">Type of the aggregate.</param>
        /// <returns>True if a snapshot should be created, false otherwise.</returns>
        public async Task<bool> IsSnapshotNeededAsync(Guid aggregateId, Type aggregateType)
        {
            using (var ctx = new EventStoreDbContext(_configuration))
            {
                return await ctx.Set<Event>().Where(e => e.AggregateType == aggregateType.AssemblyQualifiedName
                && e.AggregateId == aggregateId).CountAsync().ConfigureAwait(false) >= _nbEvents;
            }
        }

        #endregion

    }
}
