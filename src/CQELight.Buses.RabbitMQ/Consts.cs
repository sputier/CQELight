using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ
{
    /// <summary>
    /// Holder of many app consts.
    /// </summary>
    static class Consts
    {

        #region Consts

        /// <summary>
        /// Routing key for events.
        /// </summary>
        internal static readonly string CONST_EVENTS_ROUTING_KEY = "cqe_events";
        /// <summary>
        /// Routing key for events.
        /// </summary>
        internal static readonly string CONST_EVENTS_EXCHANGE_NAME = "cqe_events_exchange";
        /// <summary>
        /// Queue name for commands.
        /// </summary>
        internal static readonly string CONST_COMMANDS_QUEUE_NAME = "cqe_commands";
        /// <summary>
        /// Key for header that contains event's type.
        /// </summary>
        internal static readonly string CONST_HEADER_KEY_EVENT_TYPE = "eventType";
        /// <summary>
        /// Key for header that contains command's type.
        /// </summary>
        internal static readonly string CONST_HEADER_KEY_COMMAND_TYPE = "commandType";
        /// <summary>
        /// Name of the queue for deadletter messages.
        /// </summary>
        internal static readonly string CONST_QUEUE_NAME_DEAD_LETTER = "cqe_non_treated_events";

        #endregion

    }
}
