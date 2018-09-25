using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.AzureServiceBus
{
    internal static class Consts
    {

        #region Consts

        internal static readonly string CONST_CQE_EXCHANGE_NAME = "cqe_exchange";
        internal static readonly string CONST_HEADER_KEY_EVENT_TYPE = "eventType";
        internal static readonly string CONST_HEADER_KEY_COMMAND_TYPE = "commandType";

        internal static readonly string CONST_QUEUE_NAME_DEAD_QUEUE = "cqe_non_treated_messages";
        internal static readonly string CONST_DEAD_LETTER_QUEUE_PREFIX = "cqe_dlq_";
        internal static readonly string CONST_ROUTING_KEY_ALL = "cqe_routing_all";
        internal static readonly string CONST_QUEUE_NAME_PREFIX = "cqe_appqueue_";

        #endregion

    }
}
