﻿using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.RabbitMQ;
using CQELight.Buses.RabbitMQ.Client;
using CQELight.Buses.RabbitMQ.Server;
using CQELight.IoC;
using System.Linq;

namespace CQELight
{
    public static class BootstrapperExtensions
    {
        #region Public static methods

        /// <summary>
        /// Use RabbitMQ client to publish events and commands to a rabbitMQ instance.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="configuration">Configuration to use RabbitMQ.</param>
        /// <returns>Bootstrapper instance.</returns>
        public static Bootstrapper UseRabbitMQClientBus(this Bootstrapper bootstrapper, RabbitMQClientBusConfiguration configuration = null)
        {
            var service = RabbitMQBootstrappService.Instance;

            service.BootstrappAction += (ctx) =>
            {
                RabbitMQClient.s_configuration = configuration ?? RabbitMQClientBusConfiguration.Default;
                if (ctx.IsServiceRegistered(BootstrapperServiceType.IoC))
                {
                    bootstrapper.AddIoCRegistrations(
                        new TypeRegistration(typeof(RabbitMQEventBus), typeof(IDomainEventBus)),
                        new InstanceTypeRegistration(configuration ?? RabbitMQClientBusConfiguration.Default,
                            typeof(RabbitMQClientBusConfiguration), typeof(AbstractBaseConfiguration)));
                    RegisterRabbitClientWithinContainer(bootstrapper);
                }
            };

            if (!bootstrapper.RegisteredServices.Any(s => s == service))
            {
                bootstrapper.AddService(service);
            }
            return bootstrapper;
        }

        /// <summary>
        /// Use RabbitMQ Server to listen from events on a specific rabbitMQ instance.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance.</param>
        /// <param name="configuration">Configuration to use RabbitMQ</param>
        /// <returns>Bootstrapper instance</returns>
        public static Bootstrapper UseRabbitMQServer(this Bootstrapper bootstrapper, RabbitMQServerConfiguration configuration = null)
        {
            var service = RabbitMQBootstrappService.Instance;

            service.BootstrappAction += (ctx) =>
            {
                RabbitMQClient.s_configuration = configuration ?? RabbitMQServerConfiguration.Default;
                if (ctx.IsServiceRegistered(BootstrapperServiceType.IoC))
                {
                    bootstrapper.AddIoCRegistrations(
                          new TypeRegistration(typeof(RabbitMQServer), typeof(RabbitMQServer)),
                          new InstanceTypeRegistration(configuration ?? RabbitMQServerConfiguration.Default,
                              typeof(RabbitMQServerConfiguration), typeof(AbstractBaseConfiguration)));
                    RegisterRabbitClientWithinContainer(bootstrapper);
                }
            };

            if (!bootstrapper.RegisteredServices.Any(s => s == service))
            {
                bootstrapper.AddService(service);
            }
            return bootstrapper;
        }

        #endregion

        #region Private methods

        private static void RegisterRabbitClientWithinContainer(Bootstrapper bootstrapper)
        {
            bootstrapper.AddIoCRegistration(new TypeRegistration<RabbitMQClient>(true));
        }

        #endregion

    }
}
