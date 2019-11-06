using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Configuration.Publisher
{
    public class RabbitPublisherEventConfiguration : BasePublisherConfiguration
    {
        #region Ctor

        public RabbitPublisherEventConfiguration(
            params Type[] types)
        {
            Types = types;
        }

        #endregion

        #region Public methods

        public RabbitPublisherEventConfiguration UseExchange(string exchangeName)
        {
            SetExchangeName(exchangeName);
            return this;
        }

        #endregion

    }
}
