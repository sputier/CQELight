using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.RabbitMQ
{
    internal class RabbitMQBootstrappService : IBootstrapperService
    {
        #region Static members

        private static RabbitMQBootstrappService _instance;

        internal static RabbitMQBootstrappService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RabbitMQBootstrappService();
                }
                return _instance;
            }
        }

        #endregion

        #region IBoostrapperService

        public BootstrapperServiceType ServiceType => BootstrapperServiceType.Bus;

        public Action BootstrappAction
        {
            get; internal set;
        }

        #endregion

    }
}
