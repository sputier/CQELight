using CQELight.Tools;
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
        private IEnumerable<Type> _iocRegisteredTypes;

        #endregion

        #region Ctor

        /// <summary>
        /// Default ctor.
        /// </summary>
        /// <param name="registeredServices">Collection of registered services.</param>
        internal BootstrappingContext(
            IEnumerable<BootstrapperServiceType> registeredServices,
            IEnumerable<Type> iocRegisteredTypes)
        {
            _registeredServices = registeredServices;
            _iocRegisteredTypes = iocRegisteredTypes;
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

        /// <summary>
        /// Check if a type has been registered into Bootstrapper IoC list
        /// at least one. This DOESN'T check custom IoC plugin registrations.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if type as at least one registration, false otherwise.</returns>
        public bool IsAbstractionRegisteredInIoC(Type type)
            => _iocRegisteredTypes?.Any(t => new TypeEqualityComparer().Equals(type, t)) == true;

        #endregion

    }
}
