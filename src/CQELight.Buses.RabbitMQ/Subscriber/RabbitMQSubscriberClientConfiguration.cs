using CQELight.Buses.RabbitMQ.Subscriber.Configuration;
using CQELight.Events.Serializers;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Subscriber
{
    /// <summary>
    /// Configuration class to setup RabbitMQ server behavior.
    /// </summary>
    public class RabbitMQSubscriberClientConfiguration : AbstractBaseConfiguration
    {
        #region Static members

        /// <summary>
        /// Default configuration that targets localhost for messaging.
        /// </summary>
        public static RabbitMQSubscriberClientConfiguration Default
            => new RabbitMQSubscriberClientConfiguration("default",
                new ConnectionFactory
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest"
                });


        #endregion

        #region Properties

        public RabbitSubscriberConfiguration SubscriberConfiguration { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new server configuration on a rabbitMQ server.
        /// </summary>
        /// <param name="emiter">Id/Name of application that is using the bus</param>
        /// <param name="connectionFactory">Configured connection factory</param>
        /// <param name="configuration">Configuration to use.</param>
        public RabbitMQSubscriberClientConfiguration(string emiter,
                                           ConnectionFactory connectionFactory,
                                           RabbitSubscriberConfiguration configuration = null)
            : base(emiter, connectionFactory, null, null)
        {
            SubscriberConfiguration = configuration ?? RabbitSubscriberConfiguration.GetDefault(emiter, connectionFactory);
        }

        #endregion

    }
}
