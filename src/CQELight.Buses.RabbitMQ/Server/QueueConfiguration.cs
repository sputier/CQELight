using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Server
{
    /// <summary>
    /// Transport of queue information.
    /// </summary>
    public struct QueueConfiguration
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
        /// The exchange key listened by the queue.
        /// </summary>
        public string ExchangeKey { get; private set; }
        /// <summary>
        /// Flag that indicates if a dead letter queue should be used.
        /// </summary>
        public bool CreateAndUseDeadLetterQueue { get; private set; }

        #endregion

        #region Ctor

        public QueueConfiguration(string queueName, string exchangeKey, bool dispatchInMemory = true, Action<object> callback = null,
            bool createAndUseDeadLetterQueue = false)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }
            QueueName = queueName;
            DispatchInMemory = dispatchInMemory;
            Callback = callback;
            ExchangeKey = exchangeKey;
            CreateAndUseDeadLetterQueue = createAndUseDeadLetterQueue;
        }

        #endregion

    }
}
