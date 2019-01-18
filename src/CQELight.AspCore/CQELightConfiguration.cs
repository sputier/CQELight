using CQELight.Bootstrapping.Notifications;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQELight.AspCore
{
    public static class CQELightConfiguration
    {

        #region Public static methods

        /// <summary>
        /// Allow an AspCore to easily configure CQELight's bootstrapper.
        /// Note that this will use AspCore dependency injection system if not
        /// IoC extensions are registered
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="bootstrapperAction">Bootstrapper configuration action.</param>
        /// <param name="optimal">Specify that bootstrapping should check that system has been configured in an optimal way</param>
        /// <param name="strict">Specifiy that bootstrapping should apply strict verification of configuration</param>
        /// <param name="throwOnError">Specifiy that if bootsrapper generates any error notification, it should throw exception instead of just returning</param>
        public static IEnumerable<BootstrapperNotification> AddCQELight(this IServiceCollection services,
            Action<Bootstrapper> bootstrapperAction,
            bool strict = false,
            bool optimal = false,
            bool throwOnError = false)
        {
            if (bootstrapperAction == null)
            {
                throw new ArgumentNullException(nameof(bootstrapperAction));
            }
            var bootstrapper = new Bootstrapper(strict, optimal, throwOnError);
            bootstrapperAction(bootstrapper);
            if (!bootstrapper.RegisteredServices.Any(s => s.ServiceType == BootstrapperServiceType.IoC))
            {
                //TODO use Microsoft.Extensions.DependencyInjection
            }

            return bootstrapper.Bootstrapp();
        }

        #endregion

    }
}
