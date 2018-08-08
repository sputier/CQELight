using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Buses.MSMQ
{
    public static class BootstrapperExtensions
    {
        #region Public static methods

        /// <summary>
        /// Use MSMQ as bus for messaging.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <returns>Bootstrapper instance.</returns>
        public static Bootstrapper UseMSMQClientBus(this Bootstrapper bootstrapper)
        {
            var service = MSMQBootstrappService.Instance;

            service.BootstrappAction = () =>
             {

             };

            if (!bootstrapper.RegisteredServices.Any(s => s == service))
            {
                bootstrapper.AddService(service);
            }

            return bootstrapper;
        }

        #endregion

    }
}
