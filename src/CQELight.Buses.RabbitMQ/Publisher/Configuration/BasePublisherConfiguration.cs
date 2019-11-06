using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Configuration.Publisher
{
    /// <summary>
    /// Base class for publisher configuration.
    /// </summary>
    public abstract class BasePublisherConfiguration
    {
        #region Properties

        /// <summary>
        /// Concerned types.
        /// </summary>
        public Type[] Types { get; protected set; }

        /// <summary>
        /// Exchange to use when publishing.
        /// </summary>
        public string ExchangeName { get; protected set; }

        #endregion

        #region Protected methods

        protected void SetExchangeName(string exchangeName)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
            {
                throw new ArgumentException("UseExchange() : Exchange should be provided",
                    nameof(exchangeName));
            }
            ExchangeName = exchangeName;
        }

        #endregion

    }
}
