using CQELight.Abstractions.Dispatcher.Configuration;
using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Dispatcher.Configuration.Internal
{
    /// <summary>
    /// Internal class to help managing configuration to build.
    /// </summary>
    internal class EventDispatchConfiguration : BaseEventDispatchConfiguration
    {

        #region Properties

        /// <summary>
        /// Flag that indicates if event is security critical.
        /// </summary>
        public bool IsSecurityCritical { get; set; }
        /// <summary>
        /// Type of event configuration is about.
        /// </summary>
        public Type EventType { get; set; }
        /// <summary>
        /// Collection of buses types for dispatch.
        /// </summary>
        public IEnumerable<Type> BusesTypes { get; set; }

        #endregion

    }
}
