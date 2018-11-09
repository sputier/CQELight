using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ
{
    /// <summary>
    /// Base class for configuration
    /// </summary>
    public abstract class AbstractBaseConfiguration : BaseEventBusConfiguration
    {
        #region Properties

        /// <summary>
        /// Host to connect to RabbitMQ.
        /// </summary>
        public string Host { get; protected set; }
        /// <summary>
        /// Port to use on RabbitMQ host.
        /// </summary>
        public int? Port { get; protected set; }
        /// <summary>
        /// User name to connect.
        /// </summary>
        public string UserName { get; protected set; }
        /// <summary>
        /// Password to connect.
        /// </summary>
        public string Password { get; protected set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new configuration for connecting to a rabbitMQ server.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="userName">The username to use.</param>
        /// <param name="password">The password to use.</param>
        /// <param name="eventsLifetime">Definition of events life time. If null, default
        /// is applied, which means that every event type has a lifetime of 1 day.</param>
        /// <param name="parallelDispatchEventTypes">Event types that allows parallel dispatch.</param>
        protected AbstractBaseConfiguration(string host, string userName, string password,
            IEnumerable<EventLifeTimeConfiguration> eventsLifetime, IEnumerable<Type> parallelDispatchEventTypes)
            : base(eventsLifetime, parallelDispatchEventTypes)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("AbstractBaseConfiguration.Ctor() : Host should be provided.", nameof(host));
            }

            UserName = userName;
            Password = password;
            if (host.Contains(":"))
            {
                var hostData = host.Split(':');
                if (hostData.Length != 2)
                {
                    throw new ArgumentException("AbstractBaseConfiguration.Ctor() : When specifying port to host, format should be 'host:port' only.");
                }
                if (!int.TryParse(hostData[1], out int port))
                {
                    throw new ArgumentException($"AbstractBaseConfiguration.Ctor() : The specified port is not a valid integer. {hostData[1]}");
                }
                Host = hostData[0];
                Port = port;
            }
            else
            {
                Host = host;
            }
        }

        #endregion

    }
}
