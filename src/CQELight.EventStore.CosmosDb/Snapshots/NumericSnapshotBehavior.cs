using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore.CosmosDb.Common;
using CQELight.EventStore.CosmosDb.Models;
using CQELight.Tools.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;

namespace CQELight.EventStore.CosmosDb.Snapshots
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

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="nbEvents">Limit of events</param>
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

        /// <summary>
        /// Generate snapshot
        /// </summary>
        /// <param name="aggregateId">aggregate id of which one we want to snapshot</param>
        /// <param name="aggregateType">Aggregate's type to snapshot</param>
        /// <returns><see cref="Task"/></returns>
        public async Task<(ISnapshot Snapshot, int NewSequence, IEnumerable<IDomainEvent> ArchiveEvents)>
            GenerateSnapshotAsync(Guid aggregateId, Type aggregateType, ISnapshot previousSnapshot = null)
        {
            Snapshot snap = null;
            const int newSequence = 1;
            List<IDomainEvent> events = new List<IDomainEvent>();
            if (aggregateType.CreateInstance() is IEventSourcedAggregate aggregateInstance)
            {
                events = EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.EventsDatabaseLink)
                  .Where(@event => @event.AggregateId == aggregateId && @event.AggregateType == aggregateType.AssemblyQualifiedName)
                  .Select(EventStoreManager.GetRehydratedEventFromDbEvent).ToList();


                aggregateInstance.RehydrateState(events);

                object stateProp =
                    aggregateType.GetAllProperties().FirstOrDefault(p => p.PropertyType.IsSubclassOf(typeof(AggregateState)))
                    ??
                    (object)aggregateType.GetAllFields().FirstOrDefault(f => f.FieldType.IsSubclassOf(typeof(AggregateState)));

                AggregateState state = null;
                if (stateProp is PropertyInfo propInfo)
                {
                    state = propInfo.GetValue(aggregateInstance) as AggregateState;
                }
                else if (stateProp is FieldInfo fieldInfo)
                {
                    state = fieldInfo.GetValue(aggregateInstance) as AggregateState;
                }

                if (state != null)
                {
                    snap = new Snapshot(
                      aggregateId: aggregateId,
                      aggregateType: aggregateType.AssemblyQualifiedName,
                      aggregateState: state,
                      snapshotBehaviorType: typeof(NumericSnapshotBehavior).AssemblyQualifiedName,
                      snapshotTime: DateTime.Now);

                    var feedResponse = await EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.EventsDatabaseLink)
                        .Where(@event => @event.AggregateId == aggregateId && @event.AggregateType == aggregateType.AssemblyQualifiedName)
                        .AsDocumentQuery().ExecuteNextAsync<Document>().ConfigureAwait(false);
                    await feedResponse
                        .DoForEachAsync(async e => await EventStoreAzureDbContext.Client.DeleteDocumentAsync(documentLink: e.SelfLink).ConfigureAwait(false))
                            .ConfigureAwait(false);

                }
            }
            return (snap, newSequence, events);
        }

        /// <summary>
        /// Define if a snapshot is need for the aggregate in parameter
        /// </summary>
        /// <param name="aggregateId">Aggregate id</param>
        /// <param name="aggregateType">Aggregate type</param>
        /// <returns><see cref="Task"/></returns>
        public async Task<bool> IsSnapshotNeededAsync(Guid aggregateId, Type aggregateType)
        {
            return (await EventStoreAzureDbContext.Client.CreateDocumentQuery<Event>(EventStoreAzureDbContext.EventsDatabaseLink)
                   .Where(@event => @event.AggregateId == aggregateId && @event.AggregateType == aggregateType.AssemblyQualifiedName)
                   .CountAsync().ConfigureAwait(false)) >= _nbEvents;
        }

        #endregion
    }
}
