using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ
{
    /// <summary>
    /// Instance of RabbitMQClient.
    /// Use it to do custom advanced scenario when you need to call
    /// RabbitMQ directly.
    /// </summary>
    public class RabbitMQClient
    {
        #region Members

        private readonly AbstractBaseConfiguration _configuration;
        private static object s_threadSafety = new object();
        private static RabbitMQClient s_instance;

        internal static AbstractBaseConfiguration s_configuration;

        #endregion

        #region Properties

        /// <summary>
        /// Access RabbitMQClient instance.
        /// Note : accessing this singleton instance is NOT recommended if you use
        /// IoC. You should inject it in your constructor to avoid issues.
        /// </summary>
        public static RabbitMQClient Instance
        {
            get
            {
                if(s_instance == null)
                {
                    lock(s_threadSafety)
                    {
                        if(s_instance == null)
                        {
                            s_instance = new RabbitMQClient(s_configuration);
                        }
                    }
                }
                return s_instance;
            }
        }

        #endregion

        #region Ctor

        internal RabbitMQClient(AbstractBaseConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Retrieves a new connection to RabbitMQ server, according to current configuration.
        /// </summary>
        /// <returns>RabbitMQ connection</returns>
        public IConnection GetConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration.Host,
                UserName = _configuration.UserName,
                Password = _configuration.Password
            };
            if (_configuration.Port.HasValue)
            {
                factory.Port = _configuration.Port.Value;
            }
            return factory.CreateConnection();
        }
        
        #endregion
    }
}
