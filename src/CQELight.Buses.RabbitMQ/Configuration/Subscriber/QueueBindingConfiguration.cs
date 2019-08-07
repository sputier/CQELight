using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Configuration.Subscriber
{
    public class QueueBindingConfiguration
    {
        #region Members

        public QueueConfiguration QueueConfiguration { get; private set; }
        public string ExchangeName { get; private set; }

        #endregion

        #region Ctor

        public QueueBindingConfiguration(
            QueueConfiguration queueConfiguration,
            string exchangeName
            )
        {
            ExchangeName = exchangeName;
            QueueConfiguration = queueConfiguration;
        }

        #endregion
    }
}
