using CQELight.Abstractions.IoC.Interfaces;
using CQELight.Bootstrapping.Notifications;
using CQELight.IoC;
using CQELight.IoC.Microsoft.Extensions.DependencyInjection;
using CQELight.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQELight
{
    public static class BootstrapperExtensions
    {

        #region Public static methods

        public static Bootstrapper UseMicrosoftDependencyInjection(this Bootstrapper bootstrapper,
            IServiceCollection services, params string[] excludedDllsForAutoRegistration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var service = new MicrosoftDependencyInjectionService();

            service.BootstrappAction = (ctx) =>
            {
                AddComponentRegistrationToContainer(services, bootstrapper.IoCRegistrations);
                AddAutoRegisteredTypes(bootstrapper, services, excludedDllsForAutoRegistration);
                DIManager.Init(new MicrosoftScopeFactory(services));
            };

            bootstrapper.AddService(service);
            return bootstrapper;
        }

        #endregion

        #region Private static methods

        private static void AddAutoRegisteredTypes(Bootstrapper bootstrapper, IServiceCollection services, string[] excludedDllsForAutoRegistration)
        {
            bool CheckPublicConstructorAvailability(Type type)
            {
                if (!type.GetConstructors().Any(c => c.IsPublic))
                {
                    bootstrapper.AddNotification(new BootstrapperNotification(BootstrapperNotificationType.Error, "You must provide public constructor to Microsoft.Extensions.DependencyInjection extension cause it only supports public constructor. If you want to use internal or private constructor, switch to another IoC provider, such as Autofac"));
                    return false;
                }
                return true;
            }

            foreach (var type in ReflectionTools.GetAllTypes(excludedDllsForAutoRegistration)
                .Where(t => typeof(IAutoRegisterType).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract).ToList())
            {
                if (CheckPublicConstructorAvailability(type))
                {
                    services.AddTransient(type, type);
                    foreach (var @interface in type.GetInterfaces())
                    {
                        services.AddTransient(@interface, type);
                    }
                }
            }

            foreach (var type in ReflectionTools.GetAllTypes(excludedDllsForAutoRegistration)
                .Where(t => typeof(IAutoRegisterTypeSingleInstance).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract).ToList())
            {
                if (CheckPublicConstructorAvailability(type))
                {
                    services.AddSingleton(type, type);
                    foreach (var @interface in type.GetInterfaces())
                    {
                        services.AddSingleton(@interface, type);
                    }
                }
            }
        }

        private static void AddComponentRegistrationToContainer(IServiceCollection services, IEnumerable<ITypeRegistration> customRegistration)
        {
            if (customRegistration?.Any() == false)
            {
                return;
            }
            foreach (var item in customRegistration)
            {
                if (item is InstanceTypeRegistration instanceTypeRegistration)
                {
                    foreach (var type in item.AbstractionTypes)
                    {
                        services.AddScoped(type, _ => instanceTypeRegistration.Value);
                    }
                }
                if (item is TypeRegistration typeRegistration)
                {
                    foreach (var type in item.AbstractionTypes)
                    {
                        services.AddScoped(type, typeRegistration.InstanceType);
                    }
                }
                if (item is FactoryRegistration factoryRegistration)
                {
                    foreach (var type in item.AbstractionTypes)
                    {
                        services.AddScoped(type, _ => factoryRegistration.Factory());
                    }
                }
            }
        }


        #endregion

    }
}
