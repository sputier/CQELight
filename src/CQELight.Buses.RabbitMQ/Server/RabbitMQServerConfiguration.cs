using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Server
{
    /// <summary>
    /// Configuration class to setup RabbitMQ server behavior.
    /// </summary>
    public class RabbitMQServerConfiguration
    {

        #region Static members

        /// <summary>
        /// Default configuration that targets localhost for messaging.
        /// </summary>
        public static RabbitMQServerConfiguration Default
            => new RabbitMQServerConfiguration("localhost", "_event_server_default_queue");

        #endregion

        #region Properties

        /// <summary>
        /// Host to connect to RabbitMQ.
        /// </summary>
        public string Host { get; private set; }
        /// <summary>
        /// Name of the queue.
        /// </summary>
        public string QueueName { get; private set; }
        /// <summary>
        /// Flag that indicates if queue should be deleted when server is disposed.
        /// </summary>
        public bool DeleteQueueOnDispose { get; private set; }
        /// <summary>
        /// Flag to indicates if creating and using a dead letter queue, which is a queue that will holds
        /// all message that haven't been correctly processed on server side (callback is throwing exception).
        /// </summary>
        public bool CreateAndUseDeadLetterQueue { get; private set; }

        #endregion

        #region Ctor

        public RabbitMQServerConfiguration(string host, string queueName, bool deleteQueueOnDispose = false,
            bool createAndUseDeadLetterQueue = true)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("RabbitMQServerConfiguration.Ctor() : Host should be provided.", nameof(host));
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException("RabbitMQServerConfiguration.Ctor() : Queue name should be provided.", nameof(queueName));
            }

            Host = host;
            QueueName = queueName;
            DeleteQueueOnDispose = deleteQueueOnDispose;
            CreateAndUseDeadLetterQueue = createAndUseDeadLetterQueue;
        }

        #endregion

    }
}
