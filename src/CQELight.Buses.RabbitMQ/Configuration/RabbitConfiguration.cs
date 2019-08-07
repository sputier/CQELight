using CQELight.Buses.RabbitMQ.Configuration.Publisher;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Configuration
{
    public class RabbitConfiguration
    {
        #region Properties

        public RabbitPublisherConfiguration PublisherConfiguration { get; set; }
        public RabbitSubscriberConfiguration SubscriberConfiguration { get; set; }

        #endregion
    }
}
