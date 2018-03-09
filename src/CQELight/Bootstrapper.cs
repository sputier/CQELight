using CQELight.Dispatcher;
using CQELight.Dispatcher.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight
{
    /// <summary>
    /// Bootstrapping class for system initialisation.
    /// </summary>
    public class Bootstrapper
    {

        #region Ctor

        /// <summary>
        /// Create a new bootstrapper for the application.
        /// </summary>
        public Bootstrapper()
        {

        }

        #endregion

        #region Public methods 

        /// <summary>
        /// Configure the system dispatcher with the following configuration.
        /// </summary>
        /// <param name="dispatcherConfiguration">Configuration to use.</param>
        /// <returns>Instance of the boostraper</returns>
        public Bootstrapper ConfigureDispatcher(CoreDispatcherConfiguration dispatcherConfiguration)
        {
            if (dispatcherConfiguration == null)
            {
                throw new ArgumentNullException(nameof(dispatcherConfiguration));
            }
            CoreDispatcher.UseConfiguration(dispatcherConfiguration);
            return this;
        }

        #endregion

    }
}
