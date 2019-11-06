using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Configuration.Publisher
{
    /// <summary>
    /// Configuration for command publishing.
    /// </summary>
    public class RabbitPublisherCommandConfiguration : BasePublisherConfiguration
    {
        #region Ctor

        /// <summary>
        /// Creates a new configuration for a bunch of command types.
        /// </summary>
        /// <param name="commandTypes">Concerned command types.</param>
        public RabbitPublisherCommandConfiguration(
            params Type[] commandTypes)
        {
            Types = commandTypes;
        }

        #endregion

        #region Public methods
        
        /// <summary>
        /// Specify the exchange to use when publishing commands defined in this configuration.
        /// </summary>
        /// <param name="exchangeName">Name of the exchange to use.</param>
        /// <returns>Current configured instance</returns>
        public RabbitPublisherCommandConfiguration UseExchange(string exchangeName)
        {
            SetExchangeName(exchangeName);
            return this;
        }

        #endregion
    }
}
