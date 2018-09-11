using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQELight.Abstractions.EventStore
{
    /// <summary>
    /// Base class for event sourced aggregates,
    /// that extends capabilities of standard DDD aggregate to 
    /// allow management by an event store.
    /// </summary>
    /// <typeparam name="T">Type of aggregate Id</typeparam>
    public abstract class EventSourcedAggregate<T> : AggregateRoot<T>, IEventSourcedAggregate
    {

        #region Properties members

        /// <summary>
        /// Current state of the aggregate.
        /// </summary>
        protected virtual AggregateState State { get; set; }

        #endregion
        
        #region IEventSourcedAggregate
        
        /// <summary>
        /// Rehydratation method that needs to be overriden in order to set back state
        /// to a good value, based on a collection of events.
        /// </summary>
        /// <param name="events">Events used to recreate the state.</param>
        public virtual void RehydrateState(IEnumerable<IDomainEvent> events) => State?.ApplyRange(events);

        #endregion

    }
}
