using Autofac;
using CQELight.Abstractions.IoC.Interfaces;
using CQELight.IoC;
using CQELight.IoC.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight
{
    public static class BootstrapperExt
    {
        #region Public static methods

        /// <summary>
        /// Configure the bootstrapper to use Autofac as IoC.
        /// </summary>
        /// <param name="bootstrapper">Instance of boostrapper.</param>
        /// <param name="containerBuilder">Autofac containerbuilder that has been configured according to app..</param>
        /// <param name="excludedAutoRegisterTypeDLLs">DLLs name to exclude from auto-configuration into IoC
        /// (IAutoRegisterType will be ineffective).</param>
        public static Bootstrapper UseAutofacAsIoC(this Bootstrapper bootstrapper, ContainerBuilder containerBuilder,
            params string[] excludedAutoRegisterTypeDLLs)
        {
            if (containerBuilder == null)
            {
                throw new ArgumentNullException(nameof(containerBuilder));
            }
            var service = new AutofacBootstrappService
            {
                BootstrappAction = (ctx) => CreateConfigWithContainer(bootstrapper, containerBuilder, excludedAutoRegisterTypeDLLs)
            };
            bootstrapper.AddService(service);
            return bootstrapper;
        }

        /// <summary>
        /// Configure the bootstrapper to use Autofac as IoC.
        /// </summary>
        /// <param name="bootstrapper">Instance of boostrapper.</param>
        /// <param name="containerBuilderConfiguration">Configuration to apply on freshly created container builder.</param>
        /// <param name="excludedAutoRegisterTypeDLLs">DLLs name to exclude from auto-configuration into IoC
        /// (IAutoRegisterType will be ineffective).</param>
        public static Bootstrapper UseAutofacAsIoC(this Bootstrapper bootstrapper, Action<ContainerBuilder> containerBuilderConfiguration,
            params string[] excludedAutoRegisterTypeDLLs)
        {
            var service = new AutofacBootstrappService
            {
                BootstrappAction = (ctx) =>
                {
                    var containerBuilder = new ContainerBuilder();
                    containerBuilderConfiguration?.Invoke(containerBuilder);
                    CreateConfigWithContainer(bootstrapper, containerBuilder, excludedAutoRegisterTypeDLLs);
                }
            };
            bootstrapper.AddService(service);
            return bootstrapper;
        }

        #endregion

        #region Private static methods

        private static void CreateConfigWithContainer(Bootstrapper bootstrapper, ContainerBuilder containerBuilder, string[] excludedAutoRegisterTypeDLLs)
        {
            containerBuilder.RegisterModule(new AutoRegisterModule(excludedAutoRegisterTypeDLLs));
            AddComponentRegistrationToContainer(containerBuilder, bootstrapper.IoCRegistrations);
            containerBuilder.Register(c => AutofacScopeFactory.Instance).AsImplementedInterfaces();

            var container = containerBuilder.Build();
            var factory = new AutofacScopeFactory(container);

            DIManager.Init(factory);
        }

        private static void AddComponentRegistrationToContainer(ContainerBuilder containerBuilder, IEnumerable<ITypeRegistration> customRegistration)
        {
            if (customRegistration?.Any() == false)
            {
                return;
            }
            var fullCtorFinder = new FullConstructorFinder();
            foreach (var item in customRegistration)
            {
                if (item is InstanceTypeRegistration instanceTypeRegistration)
                {
                    containerBuilder.Register(c => instanceTypeRegistration.Value)
                        .As(instanceTypeRegistration.AbstractionTypes.ToArray());
                }
                else if (item is TypeRegistration typeRegistration)
                {
                    containerBuilder.RegisterType(typeRegistration.InstanceType)
                        .As(typeRegistration.AbstractionTypes.ToArray())
                        .FindConstructorsWith(fullCtorFinder);
                }
                else if (item is FactoryRegistration factoryRegistration)
                {
                    containerBuilder.Register(c => factoryRegistration.Factory.Invoke())
                        .As(factoryRegistration.AbstractionTypes.ToArray());
                }
            }
        }

        #endregion

    }
}
