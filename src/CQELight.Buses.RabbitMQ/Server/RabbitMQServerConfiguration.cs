using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Server
{
    /// <summary>
    /// Configuration class to setup RabbitMQ server behavior.
    /// </summary>
    public class RabbitMQServerConfiguration
    {

        #region Static members

        /// <summary>
        /// Default configuration that targets localhost for messaging.
        /// </summary>
        public static RabbitMQServerConfiguration Default
            => new RabbitMQServerConfiguration("localhost", "_event_server_default_queue");

        #endregion

        #region Properties

        /// <summary>
        /// Host to connect to RabbitMQ.
        /// </summary>
        public string Host { get; private set; }
        /// <summary>
        /// Name of the queue.
        /// </summary>
        public string QueueName { get; private set; }

        #endregion

        #region Ctor

        public RabbitMQServerConfiguration(string host, string queueName)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("RabbitMQServerConfiguration.Ctor() : Host should be provided.", nameof(host));
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException("RabbitMQServerConfiguration.Ctor() : Queue name should be provided.", nameof(queueName));
            }

            Host = host;
            QueueName = queueName;
        }

        #endregion

    }
}
