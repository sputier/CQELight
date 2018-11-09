using CQELight.Buses.MSMQ.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Buses.MSMQ
{
    internal class Helpers
    {

        public static MessageQueue GetMessageQueue(string queueName = "")
        {
            var queue = string.IsNullOrWhiteSpace(queueName) ? Consts.CONST_QUEUE_NAME : queueName;
            MessageQueue messageQueue;
            if (!MessageQueue.Exists(queue))
            {
                messageQueue = MessageQueue.Create(queue);
            }
            else
            {
                messageQueue = new MessageQueue(queue);
            }

            messageQueue.Formatter = new JsonMessageFormatter();
            return messageQueue;
        }

    }
}
