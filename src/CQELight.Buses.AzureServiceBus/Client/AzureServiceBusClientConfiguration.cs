using System;
using System.Collections.Generic;

namespace CQELight.Buses.AzureServiceBus.Client
{
    /// <summary>
    /// Configuration class for Azure Service Bus.
    /// </summary>
    public class AzureServiceBusClientConfiguration : BaseEventBusConfiguration
    {

        #region Properties

        /// <summary>
        /// Connection string for Azure Service Bus.
        /// </summary>
        public string ConnectionString { get; }

        #endregion

        #region Ctor

        public AzureServiceBusClientConfiguration(string connectionString, IEnumerable<EventLifeTimeConfiguration> eventsLifetime,
            IEnumerable<Type> parallelDispatchTypes)
            : base(eventsLifetime, parallelDispatchTypes)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new System.ArgumentException("AzureServiceBusClientConfiguration.ctor() : Connection string shouldn't be null or whitespace",
                    nameof(connectionString));
            }

            ConnectionString = connectionString;
        }


        #endregion
    }
}