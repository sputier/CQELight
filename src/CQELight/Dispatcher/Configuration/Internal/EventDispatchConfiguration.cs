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
        /// Bus type for dispatch.
        /// </summary>
        public Type BusType { get; set; }

        #endregion

    }
}
