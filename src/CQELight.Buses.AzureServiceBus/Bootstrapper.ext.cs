using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.AzureServiceBus.Client;
using CQELight.IoC;
using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQELight.Buses.AzureServiceBus
{
    public static class BootstrapperExt
    {

        #region Public static methods        

        /// <summary>
        /// Use AzureServiceBus to publish events.
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper instance</param>
        /// <param name="configuration">Azure service bus configuration</param>
        /// <returns>Bootstrapper instance</returns>
        public static Bootstrapper UseAzureServiceBus(this Bootstrapper bootstrapper, AzureServiceBusClientConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentException("Bootstrapper.UseAzureServiceBus() : Configuration should be provided.", nameof(configuration));
            }

            bootstrapper.AddIoCRegistration(new InstanceTypeRegistration(
                configuration,
                typeof(AzureServiceBusClientConfiguration)));

            bootstrapper.AddIoCRegistration(new TypeRegistration(
                typeof(AzureServiceBusClient),
                typeof(AzureServiceBusClient),
                typeof(IDomainEventBus)));

            bootstrapper.AddIoCRegistration(new FactoryRegistration(() =>
                new QueueClient(configuration.ConnectionString, "CQELight"), typeof(QueueClient), typeof(IQueueClient)));

            return bootstrapper;
        }

        #endregion

    }
}