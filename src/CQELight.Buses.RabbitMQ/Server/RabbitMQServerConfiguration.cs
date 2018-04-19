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
            => new RabbitMQServerConfiguration("localhost", "guest", "guest",
                new QueueConfiguration(Consts.CONST_QUEUE_NAME_EVENTS, Consts.CONST_EVENTS_ROUTING_KEY),
                new QueueConfiguration(Consts.CONST_QUEUE_NAME_COMMANDS, Consts.CONST_COMMANDS_ROUTING_KEY));


        #endregion

        #region Properties

        /// <summary>
        /// Collection of configurer listened queues.
        /// </summary>
        public QueueConfiguration[] QueuesConfiguration { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new server configuration on a rabbitMQ server.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="userName">The username to use.</param>
        /// <param name="password">The password to use.</param>
        public RabbitMQServerConfiguration(string host, string userName, string password,
            params QueueConfiguration[] queuesConfiguration)
            : base(host, userName, password)
        {
            if (queuesConfiguration == null || queuesConfiguration.Any() == false)
            {
                throw new ArgumentException("RabbitMQServerConfiguration.ctor() : At least one queue should be listened by the server.");
            }

            QueuesConfiguration = queuesConfiguration;
        }

        #endregion

    }
}
