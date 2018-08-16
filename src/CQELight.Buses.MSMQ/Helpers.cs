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

        public static MessageQueue GetMessageQueue()
        {
            MessageQueue messageQueue;
            if (!MessageQueue.Exists(Consts.CONST_QUEUE_NAME))
            {
                messageQueue = MessageQueue.Create(Consts.CONST_QUEUE_NAME);
            }
            else
            {
                messageQueue = new MessageQueue(Consts.CONST_QUEUE_NAME);
            }

            messageQueue.Formatter = new JsonMessageFormatter();
            return messageQueue;
        }

    }
}
