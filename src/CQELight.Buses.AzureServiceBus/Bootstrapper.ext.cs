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

        public static Bootstrapper UseAzureServiceBus(this Bootstrapper bootstrapper, string connectionString,
            IEnumerable<EventLifeTimeConfiguration> eventsLifetime = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Bootstrapper.UseAzureServiceBus() : Connection string should be provided.", nameof(connectionString));
            }

            return UseAzureServiceBus(bootstrapper, new AzureServiceBusClientConfiguration(connectionString, eventsLifetime));
        }

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

            return bootstrapper;
        }

        #endregion

    }
}