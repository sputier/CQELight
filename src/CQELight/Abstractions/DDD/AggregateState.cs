using CQELight.Abstractions.Events.Interfaces;
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
            Action<IDomainEvent> apply;
            if (_handlersByType.TryGetValue(evt.GetType(), out apply))
            {
                apply(evt);
            }
        }


        #endregion

        #region Protected methods

        protected void AddHandler<T>(Action<T> when) where T : IDomainEvent
            => _handlersByType.Add(typeof(T), a => when((T)a));

        #endregion


    }
}
