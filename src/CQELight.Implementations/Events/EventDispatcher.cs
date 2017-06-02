using CQELight.Abstractions;
using MoreLinq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Implementations.Events
{
    /// <summary>
    /// Class for event dispatching.
    /// </summary>
    public class EventDispatcher
    {

        #region Static members

        /// <summary>
        /// Flag to indicate if dispatcher has been init.
        /// </summary>
        static bool s_init;

        /// <summary>
        /// Matching between domain event types and bus that match it.
        /// </summary>
        static ConcurrentDictionary<IDomainEventBus, IEnumerable<Type>> s_busesTypes = new ConcurrentDictionary<IDomainEventBus, IEnumerable<Type>>();

        #endregion

        #region Public static methods

        /// <summary>
        /// Configure a bus to dispatch only event of a certain type.
        /// </summary>
        /// <param name="bus">Bus.</param>
        /// <param name="types"></param>
        public void Configure(IDomainEventBus bus, params Type[] types)
        {
            if (null == bus)
                throw new ArgumentNullException(nameof(bus), "EventDispatcher.Configure() : Cannot add a null instance of bus.");
            s_busesTypes.AddOrUpdate(bus, types?.AsEnumerable(), (b, t) => t);
            s_init = true;
        }

        /// <summary>
        /// Dispatch a domain event within the system.
        /// </summary>
        /// <param name="event">Domain event to dispatch.</param>
        public void Dispatch(IDomainEvent @event)
        {
            if(!s_init)
            s_busesTypes.Where(m => m.Value == null || m.Value.Any(e => e.GetType() == @event.GetType()))
                ?.ForEach(b => b.Key.Dispatch(@event));
        }

        public void 

        #endregion


    }
}
