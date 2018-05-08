using CQELight.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Extensions
{
    internal static class AppIdExtensions
    {
        #region Extensions methods

        public static string ToQueueName(this AppId appId)
        {
            string queueName = Consts.CONST_QUEUE_NAME_PREFIX;
            if (!string.IsNullOrWhiteSpace(appId.Alias))
            {
                queueName += appId.Alias + "_";
            }
            queueName += appId.Value.ToString();
            return queueName;
        }

        #endregion

    }
}
