using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Events
{
    /// <summary>
    /// Configuration data for RabbitMQ event bus.
    /// </summary>
    public class RabbitMQClientEventBusConfiguration : IDomainEventBusConfiguration
    {

        #region Static members

        /// <summary>
        /// Default configuration that targets localhost for messaging.
        /// </summary>
        public static RabbitMQClientEventBusConfiguration Default
            => new RabbitMQClientEventBusConfiguration("localhost");

        #endregion

        #region Properties

        /// <summary>
        /// Host to connect to RabbitMQ.
        /// </summary>
        public string Host { get; private set; }

        #endregion

        #region Ctor

        public RabbitMQClientEventBusConfiguration(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("RabbitMQClientEventBusConfiguration.Ctor() : Host should be provided.", nameof(host));
            }
            
            Host = host;
        }

        #endregion

    }
}
