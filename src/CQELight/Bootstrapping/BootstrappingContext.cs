using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight
{
    /// <summary>
    /// Bootstrapping context that allows to perform more accurate bootstrapping.
    /// </summary>
    public class BootstrappingContext
    {

        #region Members

        private IEnumerable<BootstrapperServiceType> _registeredServices;

        #endregion

        #region Ctor

        /// <summary>
        /// Default ctor.
        /// </summary>
        /// <param name="registeredServices">Collection of registered services.</param>
        internal BootstrappingContext(
            IEnumerable<BootstrapperServiceType> registeredServices)
        {
            _registeredServices = registeredServices;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Allow to check if a specific service has been registered.
        /// </summary>
        /// <param name="type">Type of service to check.</param>
        /// <returns>True if service is registered, false otherwise.</returns>
        public bool IsServiceRegistered(BootstrapperServiceType type)
            => _registeredServices.Any(s => s == type);
        
        #endregion

    }
}
