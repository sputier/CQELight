using Autofac;
using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.IoC.Autofac
{
    public static class BootstrapperExt
    {

        #region Public static methods

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
            AddComponentRegistrationToContainer(containerBuilder, bootstrapper.IoCRegistrations);


            DIManager.Init(new AutofacScopeFactory(containerBuilder.Build()));

            return bootstrapper;
        }

        #endregion

        #region Private static methods

        private static void AddComponentRegistrationToContainer(ContainerBuilder containerBuilder, IEnumerable<ITypeRegistration> customRegistration)
        {
            if(customRegistration?.Any() == false)
            {
                return;
            }
            foreach (var item in customRegistration)
            {
                if(item is InstanceTypeRegistration instanceTypeRegistration)
                {
                    containerBuilder.Register(c => instanceTypeRegistration.Value)
                        .As(instanceTypeRegistration.Types.ToArray());
                }
                if(item is TypeRegistration typeRegistration)
                {
                    containerBuilder.RegisterType(typeRegistration.InstanceType)
                        .As(typeRegistration.Types.ToArray());
                }
                if(item is FactoryRegistration factoryRegistration)
                {
                    containerBuilder.Register(c => factoryRegistration.Factory.Invoke())
                        .As(factoryRegistration.Types.ToArray());
                }
            }
        }

        #endregion

    }
}
