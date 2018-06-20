using CQELight.Abstractions.Events.Interfaces;
using CQELight.Configuration;
using CQELight.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Buses
{
    /// <summary>
    /// Base abstract class for events bus configuration.
    /// </summary>
    public abstract class BaseEventBusConfiguration
    {

        #region Properties

        /// <summary>
        /// Configuration for events lifetimes. 
        /// </summary>
        public IEnumerable<EventLifeTimeConfiguration> EventsLifetime { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Base constructor for event bus configuration.
        /// </summary>
        /// <param name="eventsLifetime">Definition of events life time. If null, default
        /// is applied, which means that every event type has a lifetime of 1 day.</param>
        public BaseEventBusConfiguration(IEnumerable<EventLifeTimeConfiguration> eventsLifetime)
        {
            if (eventsLifetime != null)
            {
                EventsLifetime = eventsLifetime;
            }
            else
            {
                EventsLifetime = ReflectionTools.GetAllTypes()
                    .Where(t => typeof(IDomainEvent).IsAssignableFrom(t))
                    .Select(t => new
                    EventLifeTimeConfiguration(t, TimeSpan.FromDays(1)));
            }
        }

        #endregion

    }
}
