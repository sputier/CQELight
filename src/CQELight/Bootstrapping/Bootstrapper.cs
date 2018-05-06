using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Bootstrapping.Notifications;
using CQELight.Dispatcher;
using CQELight.Dispatcher.Configuration;
using CQELight.IoC;
using CQELight.Tools.Extensions;
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
        private readonly bool _checkOptimal;

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
        /// <param name="checkOptimal">Flag to indicates if optimal system is currently 'On', which means
        /// that one service of each kind should be provided.</param>
        public Bootstrapper(bool strict = false, bool checkOptimal = false)
        {
            _services = new List<IBootstrapperService>();
            _iocRegistrations = new List<ITypeRegistration>();
            _strict = strict;
            _checkOptimal = checkOptimal;
        }

        #endregion

        #region Public methods 

        /// <summary>
        /// Perform the bootstrapping of all configured services.
        /// </summary>
        /// <param name="notifications">Collection of notifications.</param>
        public void Bootstrapp(out List<BootstrapperNotification> notifications)
        {
            notifications = new List<BootstrapperNotification>();
            if (_checkOptimal)
            {
                if (!_services.Any(s => s.ServiceType == BootstrapperServiceType.Bus))
                {
                    notifications.Add(new BootstrapperNotification { Type = BootstrapperNotificationType.Warning, ContentType = BootstapperNotificationContentType.BusServiceMissing });
                }
                if (!_services.Any(s => s.ServiceType == BootstrapperServiceType.DAL))
                {
                    notifications.Add(new BootstrapperNotification { Type = BootstrapperNotificationType.Warning, ContentType = BootstapperNotificationContentType.DALServiceMissing });
                }
                if (!_services.Any(s => s.ServiceType == BootstrapperServiceType.EventStore))
                {
                    notifications.Add(new BootstrapperNotification { Type = BootstrapperNotificationType.Warning, ContentType = BootstapperNotificationContentType.EventStoreServiceMissing });
                }
                if (!_services.Any(s => s.ServiceType == BootstrapperServiceType.IoC))
                {
                    notifications.Add(new BootstrapperNotification { Type = BootstrapperNotificationType.Warning, ContentType = BootstapperNotificationContentType.IoCServiceMissing });
                }
            }
            _iocRegistrations.Add(new TypeRegistration(typeof(BaseDispatcher), typeof(IDispatcher), typeof(BaseDispatcher)));
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

            if (_strict && service.ServiceType != BootstrapperServiceType.Bus)
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
        /// Configure the system CoreDispatcher with the following configuration.
        /// All manually created dispatchers will be created using their own configuration if speciffied,
        /// or the one specified here. If this method is not called, default configuration will be used for 
        /// all dispatchers.
        /// </summary>
        /// <param name="dispatcherConfiguration">Configuration to use.</param>
        /// <returns>Instance of the boostraper</returns>
        public Bootstrapper ConfigureCoreDispatcher(DispatcherConfiguration dispatcherConfiguration)
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

        /// <summary>
        /// Add some registrations to the bootrapper for the IoC component
        /// </summary>
        /// <param name="registrations">Collection of registrations.</param>
        /// <returns>Instance of the bootstrapper.</returns>
        public Bootstrapper AddIoCRegistrations(params ITypeRegistration[] registrations)
        {
            if(registrations?.Any() == true)
            {
                registrations.DoForEach(r => AddIoCRegistration(r));
            }
            return this;
        }

        #endregion

    }
}
