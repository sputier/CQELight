using CQELight.Abstractions.Dispatcher;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Server
{
    /// <summary>
    /// A specific configuration for events.
    /// </summary>
    public class EventQueueConfiguration : QueueConfiguration
    {

        #region Ctor

        /// <summary>
        /// Ctor of an event queue configuration, used for standard CQELight event queue configuration.
        /// </summary>
        /// <param name="dispatchInMemory">Flag that indicates if data should be distached in memory.</param>
        /// <param name="callback">Callback to invoke when receving data.</param>
        /// <param name="createAndUseDeadLetterQueue">Flag that indicates if create a specific dead letter queue, which means
        /// that all unhandled data are pushed back in.</param>
        /// <param name="serializer">Serializer to use to get data from the queue.</param>
        public EventQueueConfiguration(IDispatcherSerializer serializer, bool dispatchInMemory = true, Action<object> callback = null,
            bool createAndUseDeadLetterQueue = false)
            : base(Consts.CONST_QUEUE_NAME_EVENTS, Consts.CONST_EVENTS_ROUTING_KEY, serializer, dispatchInMemory, callback, createAndUseDeadLetterQueue)
        {

        }

        #endregion

    }
}
