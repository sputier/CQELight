using CQELight.Abstractions.Events.Interfaces;
using CQELight.DAL.Attributes;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Abstractions.DDD
{
    /// <summary>
    /// Base class for handling the aggregate state.
    /// </summary>
    public abstract class AggregateState
    {
        #region Private members

        private readonly Dictionary<Type, Action<IDomainEvent>> _handlersByType
           = new Dictionary<Type, Action<IDomainEvent>>();

        private readonly List<IDomainEvent> _events 
            = new List<IDomainEvent>();

        #endregion

        #region Properties

        /// <summary>
        /// Collection of all events that have been applied to this state.
        /// </summary>
        [Ignore]
        public IEnumerable<IDomainEvent> Events => _events.AsEnumerable();

        #endregion

        #region Public methods

        /// <summary>
        /// Apply a collection of events on the state to make it to its last one.
        /// </summary>
        /// <param name="events">Events list.</param>
        public void ApplyRange(IEnumerable<IDomainEvent> events) 
            => events.OrderBy(e => e.Sequence).DoForEach(Apply);

        /// <summary>
        /// Apply a specific event on the state.
        /// </summary>
        /// <param name="evt">Event to apply.</param>
        public void Apply(IDomainEvent evt)
        {
            if (_handlersByType.TryGetValue(evt.GetType(), out Action<IDomainEvent> apply))
            {
                apply(evt);
                _events.Add(evt);
            }
        }

        /// <summary>
        /// Retrieve the state serialized.
        /// </summary>
        /// <returns>Serialized state.</returns>
        public virtual string Serialize() => this.ToJson(true);

        #endregion

        #region Protected methods

        /// <summary>
        /// Add an handler for an event.
        /// </summary>
        /// <typeparam name="T">Type of event.</typeparam>
        /// <param name="when">Action to invoke.</param>
        protected void AddHandler<T>(Action<T> when) where T : IDomainEvent
            => _handlersByType.Add(typeof(T), a => when((T)a));

        #endregion
        
    }
}
