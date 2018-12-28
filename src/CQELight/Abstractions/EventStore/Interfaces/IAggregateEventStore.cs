using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions.EventStore.Interfaces
{
    /// <summary>
    /// Contract interface for managing aggregate with the help of an event store.
    /// </summary>
    public interface IAggregateEventStore
    {
        /// <summary>
        /// Retrieve a rehydrated aggregate from its unique Id and its type.
        /// </summary>
        /// <param name="aggregateUniqueId">Aggregate unique id.</param>
        /// <param name="aggregateType">Aggregate type.</param>
        /// <returns>Rehydrated event source aggregate.</returns>
        /// <typeparam name="TId">Type of aggregate Id</typeparam>
        Task<IEventSourcedAggregate> GetRehydratedAggregateAsync<TId>(TId aggregateUniqueId, Type aggregateType);
        /// <summary>
        /// Retrieve a rehydrated aggregate from its unique Id and its type.
        /// </summary>
        /// <param name="aggregateUniqueId">Aggregate unique id.</param>
        /// <returns>Rehydrated event source aggregate.</returns>
        /// <typeparam name="TAggregate">Type of aggregate to retrieve</typeparam>
        /// <typeparam name="TId">Type of aggregate Id</typeparam>
        Task<TAggregate> GetRehydratedAggregateAsync<TAggregate, TId>(TId aggregateUniqueId)
             where TAggregate : EventSourcedAggregate<TId>, new();
    }
}
