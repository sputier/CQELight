using CQELight.Events.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Server
{
    /// <summary>
    /// Configuration class to setup RabbitMQ server behavior.
    /// </summary>
    public class RabbitMQServerConfiguration : AbstractBaseConfiguration
    {
        #region Static members

        /// <summary>
        /// Default configuration that targets localhost for messaging.
        /// </summary>
        public static RabbitMQServerConfiguration Default
            => new RabbitMQServerConfiguration("localhost", "guest", "guest", QueueConfiguration.Empty);

        #endregion

        #region Properties

        /// <summary>
        /// Specific configuration of the queue.
        /// </summary>
        public QueueConfiguration QueueConfiguration { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new server configuration on a rabbitMQ server.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="userName">The username to use.</param>
        /// <param name="password">The password to use.</param>
        /// <param name="queueConfiguration">Queue configuration.</param>
        public RabbitMQServerConfiguration(string host, string userName, string password,
            QueueConfiguration queueConfiguration)
            : base(host, userName, password, null)
        {
            QueueConfiguration = queueConfiguration ?? throw new ArgumentNullException(nameof(queueConfiguration));
        }

        #endregion

    }
}
