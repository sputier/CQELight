using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Dispatcher.Configuration
{
    /// <summary>
    /// Base class for event dispatching.
    /// </summary>
    public abstract class BaseEventDispatchConfiguration
    {
        
        #region Properties

        /// <summary>
        /// Instance of event serializer.
        /// </summary>
        public IEventSerializer Serializer { get; set; }

        /// <summary>
        /// Handler to fire if error.
        /// </summary>
        public Action<Exception> ErrorHandler { get; set; }

        #endregion


    }
}
