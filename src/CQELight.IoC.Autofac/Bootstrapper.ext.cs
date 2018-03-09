using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.IoC.Autofac
{
    public static class BootstrapperExt
    {

        /// <summary>
        /// Configure the bootstrapper to use Autofac as IoC.
        /// </summary>
        /// <param name="bootstrapper">Instance of boostrapper.</param>
        /// <param name="containerBuilder">Autofac containerbuilder that has been configured according to app..</param>
        public static Bootstrapper UseAutofacAsIoC(this Bootstrapper bootstrapper, ContainerBuilder containerBuilder)
        {
            if (containerBuilder == null)
            {
                throw new ArgumentNullException(nameof(containerBuilder));
            }

            containerBuilder.RegisterModule<AutoRegisterModule>();
            DIManager.Init(new AutofacScopeFactory(containerBuilder.Build()));

            return bootstrapper;
        }

    }
}
