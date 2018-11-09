using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.InMemory;
using CQELight.Buses.InMemory.Commands;
using CQELight.Buses.InMemory.Events;
using CQELight.Dispatcher;
using CQELight.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight
{
    public static class BootstrapperExt
    {
        /// <summary>
        /// Configure the bootstrapper to use InMemory buses for dispatching events.
        /// </summary>
        /// <param name="bootstrapper">Instance of boostrapper.</param>
        /// <param name="configuration">Configuration to use for in memory event bus.</param>
        public static Bootstrapper UseInMemoryEventBus(this Bootstrapper bootstrapper, InMemoryEventBusConfiguration configuration = null)
        {
            var service = InMemoryBusesBootstrappService.Instance;
            service.BootstrappAction += (ctx) =>
            {
                if (ctx.IsServiceRegistered(BootstrapperServiceType.IoC))
                {
                    bootstrapper.AddIoCRegistration(new TypeRegistration(typeof(InMemoryEventBus), typeof(IDomainEventBus), typeof(InMemoryEventBus)));
                    if (configuration != null)
                    {
                        bootstrapper.AddIoCRegistration(new InstanceTypeRegistration(configuration, typeof(InMemoryEventBusConfiguration)));
                    }
                }
            };
            if (!bootstrapper.RegisteredServices.Any(s => s == service))
            {
                bootstrapper.AddService(service);
            }
            return bootstrapper;
        }

        /// <summary>
        /// Configure the system to use InMemory Event bus for dipsatching events, with the provided configuration.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="configurationBuilderAction">Action to apply on builder.</param>
        /// <returns>Bootstrapper Instance.</returns>
        public static Bootstrapper UseInMemoryEventBus(this Bootstrapper bootstrapper, Action<InMemoryEventBusConfigurationBuilder> configurationBuilderAction)
        {
            if (configurationBuilderAction == null)
                throw new ArgumentNullException(nameof(configurationBuilderAction));

            var builder = new InMemoryEventBusConfigurationBuilder();
            configurationBuilderAction(builder);

            return UseInMemoryEventBus(bootstrapper, builder.Build());
        }

        /// <summary>
        /// Configure the bootstrapper to use InMemory buses for dispatching commands.
        /// </summary>
        /// <param name="bootstrapper">Instance of boostrapper.</param>
        /// <param name="configuration">Configuration to use for in memory command bus.</param>
        public static Bootstrapper UseInMemoryCommandBus(this Bootstrapper bootstrapper, InMemoryCommandBusConfiguration configuration = null)
        {
            var service = InMemoryBusesBootstrappService.Instance;
            service.BootstrappAction += (ctx) =>
            {
                if (ctx.IsServiceRegistered(BootstrapperServiceType.IoC))
                {
                    bootstrapper.AddIoCRegistration(new TypeRegistration(typeof(InMemoryCommandBus), typeof(ICommandBus), typeof(InMemoryCommandBus)));
                    if (configuration != null)
                    {
                        bootstrapper.AddIoCRegistration(new InstanceTypeRegistration(configuration, typeof(InMemoryCommandBusConfiguration)));
                    }
                }
            };
            if (!bootstrapper.RegisteredServices.Any(s => s == service))
            {
                bootstrapper.AddService(service);
            }
            return bootstrapper;
        }

        /// <summary>
        /// Configure the system to use InMemory Command bus for dipsatching commands, with the provided configuration.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="configurationBuilderAction">Action to apply on builder.</param>
        /// <returns>Bootstrapper Instance.</returns>
        public static Bootstrapper UseInMemoryCommandBus(this Bootstrapper bootstrapper, Action<InMemoryCommandBusConfigurationBuilder> configurationBuilderAction)
        {
            if (configurationBuilderAction == null)
                throw new ArgumentNullException(nameof(configurationBuilderAction));

            var builder = new InMemoryCommandBusConfigurationBuilder();
            configurationBuilderAction(builder);

            return UseInMemoryCommandBus(bootstrapper, builder.Build());
        }

    }
}
