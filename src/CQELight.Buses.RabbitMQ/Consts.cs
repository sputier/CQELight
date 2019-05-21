using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ
{
    internal static class Consts
    {

        #region Consts

        internal static readonly string CONST_CQE_EXCHANGE_NAME = "cqelight_exchange";
        internal static readonly string CONST_QUEUE_NAME_DEAD_QUEUE = "cqe_non_treated_messages";

        internal static readonly string CONST_DEAD_LETTER_QUEUE_PREFIX = "cqe_dlq_";
        internal static readonly string CONST_ROUTING_KEY_ALL = "cqelight.events.*";

        #endregion

    }
}
