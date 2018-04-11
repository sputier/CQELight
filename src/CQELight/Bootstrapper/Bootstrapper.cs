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
        private readonly bool _strict;
        private readonly List<IBootstrapperService> _services;

        #endregion

        #region Properties

        /// <summary>
        /// Collection of components registration.
        /// </summary>
        public IEnumerable<ITypeRegistration> IoCRegistrations => _iocRegistrations.AsEnumerable();
        /// <summary>
        /// Collection of registered services.
        /// </summary>
        public IEnumerable<IBootstrapperService> RegisteredServices => _services.AsEnumerable();

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new bootstrapper for the application.
        /// </summary>
        /// <param name="strict">Flag to indicates if bootstrapper should stricly validates its content.</param>
        public Bootstrapper(bool strict = false)
        {
            _services = new List<IBootstrapperService>();
            _iocRegistrations = new List<ITypeRegistration>();
            _strict = strict;
        }

        #endregion

        #region Public methods 

        /// <summary>
        /// Perform the bootstrapping of all configured services.
        /// </summary>
        public void Bootstrapp()
        {
            foreach (var service in _services.OrderByDescending(s => s.ServiceType))
            {
                service.BootstrappAction.Invoke();
            }
        }

        /// <summary>
        /// Add a service to the collection of bootstrapped services.
        /// </summary>
        /// <param name="service">Service.</param>
        public Bootstrapper AddService(IBootstrapperService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (_strict)
            {
                var currentService = _services.Find(s => s.ServiceType == service.ServiceType);
                if (currentService != null)
                {
                    throw new InvalidOperationException($"Bootstrapper.AddService() : A service of type {service.ServiceType} has already been added." +
                        $"Current registered service : {currentService.GetType().FullName}");
                }
            }
            _services.Add(service);
            return this;
        }

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
