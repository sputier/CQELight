using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ.Server
{

    class CQEBasicConsumer : EventingBasicConsumer
    {

        #region Properties

        public QueueConfiguration Configuration { get; }

        #endregion

        #region Ctor

        public CQEBasicConsumer(IModel model, QueueConfiguration configuration) : base(model)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        #endregion

    }
}
