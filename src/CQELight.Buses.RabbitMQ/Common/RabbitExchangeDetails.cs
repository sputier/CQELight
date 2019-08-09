using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Common
{
    /// <summary>
    /// Exchange details for Rabbit.
    /// </summary>
    public class RabbitExchangeDetails
    {
        #region Properties

        /// <summary>
        /// Name of the exchange.
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// Type of the exchange.
        /// </summary>
        public string ExchangeType { get; set; }

        /// <summary>
        /// If an exchange is set to durable, every message published on it will be kept after
        /// restarting rabbit instance.
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// If an exchange is set autodelete, when no
        /// </summary>
        public bool AutoDelete { get; set; } = false;

        /// <summary>
        /// Custom additionnal properties.
        /// </summary>
        public Dictionary<string, object> AdditionnalProperties { get; set; }

        #endregion
    }
}
