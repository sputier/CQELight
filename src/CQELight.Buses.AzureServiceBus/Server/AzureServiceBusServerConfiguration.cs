using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.AzureServiceBus.Server
{
    /// <summary>
    /// Configuration class to holds information of Azure Service Bus Server.
    /// </summary>
    public class AzureServiceBusServerConfiguration
    {

        #region Properties

        /// <summary>
        /// Connection string for Azure Service Bus.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Specific configuration of the queue.
        /// </summary>
        public QueueConfiguration QueueConfiguration { get; }

        #endregion

        #region Ctor

        public AzureServiceBusServerConfiguration(string connectionString,
            QueueConfiguration queueConfiguration)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new System.ArgumentException("AzureServiceBusClientConfiguration.ctor() : Connection string shouldn't be null or whitespace",
                    nameof(connectionString));
            }
            QueueConfiguration = queueConfiguration ?? throw new ArgumentNullException(nameof(queueConfiguration));
            ConnectionString = connectionString;
        }


        #endregion

    }
}
