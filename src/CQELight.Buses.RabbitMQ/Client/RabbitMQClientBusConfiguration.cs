using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Client
{
    /// <summary>
    /// Configuration data for RabbitMQ bus.
    /// </summary>
    public class RabbitMQClientBusConfiguration
    {

        #region Static members

        /// <summary>
        /// Default configuration that targets localhost for messaging.
        /// </summary>
        public static RabbitMQClientBusConfiguration Default
            => new RabbitMQClientBusConfiguration("localhost", "guest","guest");

        #endregion

        #region Properties

        /// <summary>
        /// Host to connect to RabbitMQ.
        /// </summary>
        public string Host { get; private set; }
        /// <summary>
        /// Port to use on RabbitMQ host.
        /// </summary>
        public int? Port { get; private set; }
        /// <summary>
        /// User name to connect.
        /// </summary>
        public string UserName { get; private set; }
        /// <summary>
        /// Password to connect.
        /// </summary>
        public string Password { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new client configuration on a rabbitMQ server.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="userName">The username to use.</param>
        /// <param name="password">The password to use.</param>
        public RabbitMQClientBusConfiguration(string host, string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("RabbitMQClientBusConfiguration.Ctor() : Host should be provided.", nameof(host));
            }
            
            UserName = userName;
            Password = password;
            if(host.Contains(":"))
            {
                var hostData = host.Split(':');
                if(hostData.Length != 2)
                {
                    throw new ArgumentException("RabbitMQClientBusConfiguration.Ctor() : When specifying port to host, format should be 'host:port' only.");
                }
                if(!int.TryParse(hostData[1], out int port))
                {
                    throw new ArgumentException($"RabbitMQClientBusConfiguration.Ctor() : The specified port is not a valid integer. {hostData[1]}");
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
