using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Abstractions
{
    /// <summary>
    /// Base definition for aggregate. An aggregate is an entity that manage a bunch of entities and value-objects by keeping they consistent.
    /// </summary>
    public abstract class AggregateRoot<T> : Entity<T>
    {

        #region Properties
        
        readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

        #endregion

        #region Public Methods

        /// <summary>
        /// List of domain events associated to the aggregate.
        /// </summary>
        public virtual IEnumerable<IDomainEvent> DomainEvents => _domainEvents.AsEnumerable();

        #endregion

        #region Internal methods
        
        internal void CleanDomainEvents()
        {
            _domainEvents.Clear();
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Add a domain event to the list of events.
        /// </summary>
        /// <param name="newEvent">Event to add.</param>
        protected internal virtual void AddDomainEvent(IDomainEvent newEvent)
        {
            if (newEvent != null)
            {
                _domainEvents.Add(newEvent);
            }
        }

        #endregion

    }
}
