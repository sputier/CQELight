using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Extensions
{
    static class IModelExtensions
    {

        #region Extensions methods

        public static void CreateCQEExchange(this IModel channel)
        {
            channel.ExchangeDeclare(exchange: Consts.CONST_CQE_EXCHANGE_NAME,
                                        type: ExchangeType.Fanout,
                                        durable: true,
                                        autoDelete: false);
        }

        #endregion

    }
}
