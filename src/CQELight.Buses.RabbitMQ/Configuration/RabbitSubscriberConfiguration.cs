using CQELight.Buses.RabbitMQ.Configuration.Subscriber;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Configuration
{
    public class RabbitSubscriberConfiguration
    {
        #region Member

        private readonly IEnumerable<QueueBindingConfiguration> _queueConfigurations;

        #endregion

        #region Ctor

        public RabbitSubscriberConfiguration(
            IEnumerable<QueueBindingConfiguration> queueConfigurations)
        {
            _queueConfigurations = queueConfigurations;
        }

        #endregion

    }
}
