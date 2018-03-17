using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Dispatcher;
using CQELight.Dispatcher.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight
{
    /// <summary>
    /// Bootstrapping class for system initialisation.
    /// </summary>
    public class Bootstrapper
    {

        #region Members

        private readonly List<ITypeRegistration> _iocRegistrations;

        #endregion

        #region Properties

        /// <summary>
        /// List of components registration.
        /// </summary>
        public IEnumerable<ITypeRegistration> IoCRegistrations => _iocRegistrations.AsEnumerable();

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new bootstrapper for the application.
        /// </summary>
        public Bootstrapper()
        {
            _iocRegistrations = new List<ITypeRegistration>();
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

        /// <summary>
        /// Add a custom component IoC registration into Bootstrapper for IoC component.
        /// </summary>
        /// <param name="registration">Registration to add.</param>
        /// <returns>Instance of the boostrapper</returns>
        public Bootstrapper AddIoCRegistration(ITypeRegistration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            _iocRegistrations.Add(registration);
            return this;
        }

        #endregion

    }
}
