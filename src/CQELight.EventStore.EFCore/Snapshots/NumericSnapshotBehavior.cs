using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.EFCore.Common;
using CQELight.EventStore.EFCore.Models;
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

        #endregion

        #region Ctor

        public NumericSnapshotBehavior(int nbEvents)
        {
            if (nbEvents < 2)
            {
                throw new ArgumentException("NumericSnapshotBehavior.ctor() : The number of events to generate" +
                    " a snapshot should be greater or equal to 2.");
            }
            _nbEvents = nbEvents;
        }

        #endregion

        #region ISnapshotBehavior methods

        public async Task<(ISnapshot, int)> GenerateSnapshotAsync(Guid aggregateId, Type aggregateType)
        {
            Snapshot snap = null;
            int newSequence = 1;
            var aggregateInstance = aggregateType.CreateInstance() as IEventSourcedAggregate;
            if (aggregateInstance != null)
            {
                using (var ctx = new EventStoreDbContext(EventStoreManager.DbContextConfiguration))
                {
                    var orderedEvents =
                        await ctx.Set<Event>().Where(e => e.AggregateType == aggregateType.AssemblyQualifiedName
                        && e.AggregateId == aggregateId).OrderBy(e => e.Sequence).Take(_nbEvents).ToListAsync()
                        .ConfigureAwait(false);

                    aggregateInstance.RehydrateState(orderedEvents.Select(d =>
                        d.EventData.FromJson(Type.GetType(d.EventType)) as IDomainEvent));

                    snap = new Snapshot
                    {
                        AggregateId = aggregateId,
                        AggregateType = aggregateType.AssemblyQualifiedName,
                        SnapshotBehaviorType = typeof(NumericSnapshotBehavior).AssemblyQualifiedName,
                        SnapshotTime = DateTime.Now,
                        SnapshotData = aggregateInstance.ToJson(true)
                    };

                    //TODO store events into archive database instead of removing them
                    ctx.RemoveRange(orderedEvents);
                    await ctx.SaveChangesAsync().ConfigureAwait(false);
                }

            }
            return (snap, newSequence);
        }

        public async Task<bool> IsSnapshotNeededAsync(Guid aggregateId, Type aggregateType)
        {
            using (var ctx = new EventStoreDbContext(EventStoreManager.DbContextConfiguration))
            {
                return await ctx.Set<Event>().Where(e => e.AggregateType == aggregateType.AssemblyQualifiedName
                && e.AggregateId == aggregateId).CountAsync().ConfigureAwait(false) >= _nbEvents;
            }
        }

        #endregion

    }
}
