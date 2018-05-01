using CQELight.Abstractions.Dispatcher;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Server
{
    /// <summary>
    /// Transport of queue information.
    /// </summary>
    public class QueueConfiguration
    {

        #region Properties

        /// <summary>
        /// Name of concerned queue.
        /// </summary>
        public string QueueName { get; private set; }
        /// <summary>
        /// Flag that indicates if a dispatch should be done in memory.
        /// </summary>
        public bool DispatchInMemory { get; private set; }
        /// <summary>
        /// Custom callback to invoke when retrieving data from queue.
        /// </summary>
        public Action<object> Callback { get; private set; }
        /// <summary>
        /// The routing key listened by the queue.
        /// </summary>
        public string RoutingKey { get; private set; }
        /// <summary>
        /// Flag that indicates if a dead letter queue should be used.
        /// </summary>
        public bool CreateAndUseDeadLetterQueue { get; private set; }
        /// <summary>
        /// Serializer to use to get data from queue.
        /// </summary>
        public IDispatcherSerializer Serializer { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new customisable queue listening configuration.
        /// </summary>
        /// <param name="queueName">Name of the queue to listen to.</param>
        /// <param name="routingKey">Name of the routing key.</param>
        /// <param name="serializer">Serializer to use to get data from the queue.</param>
        /// <param name="dispatchInMemory">Flag that indicates if data should be distached in memory.</param>
        /// <param name="callback">Callback to invoke when receving data.</param>
        /// <param name="createAndUseDeadLetterQueue">Flag that indicates if create a specific dead letter queue, which means
        /// that all unhandled data are pushed back in.</param>
        public QueueConfiguration(string queueName, string routingKey, IDispatcherSerializer serializer, bool dispatchInMemory = true, Action<object> callback = null,
            bool createAndUseDeadLetterQueue = false)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            if (string.IsNullOrWhiteSpace(routingKey))
            {
                throw new ArgumentNullException(nameof(routingKey));
            }

            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            QueueName = queueName;
            DispatchInMemory = dispatchInMemory;
            Callback = callback;
            RoutingKey = routingKey;
            CreateAndUseDeadLetterQueue = createAndUseDeadLetterQueue;
        }

        #endregion

    }
}
