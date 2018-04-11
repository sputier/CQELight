using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ
{

    static class Consts
    {

        #region Consts

        internal static readonly string CONST_EVENTS_ROUTING_KEY = "cqe_events";
        internal static readonly string CONST_EVENTS_EXCHANGE_NAME = "cqe_events_exchange";
        internal static readonly string CONST_COMMANDS_EXCHANGE_NAME = "cqe_commands_exchange";
        internal static readonly string CONST_COMMANDS_ROUTING_KEY = "cqe_commands";
        internal static readonly string CONST_HEADER_KEY_EVENT_TYPE = "eventType";
        internal static readonly string CONST_HEADER_KEY_COMMAND_TYPE = "commandType";
        internal static readonly string CONST_QUEUE_NAME_DEAD_LETTER_EVENTS = "cqe_non_treated_events";
        internal static readonly string CONST_QUEUE_NAME_DEAD_LETTER_COMMANDS = "cqe_non_treated_commands";

        #endregion

    }
}
