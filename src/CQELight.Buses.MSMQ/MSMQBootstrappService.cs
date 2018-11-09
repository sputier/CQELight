using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Buses.MSMQ
{
    internal class MSMQBootstrappService : IBootstrapperService
    {
        #region Static members

        private static MSMQBootstrappService _instance;

        internal static MSMQBootstrappService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MSMQBootstrappService();
                }
                return _instance;
            }
        }

        #endregion

        #region IBoostrapperService

        public BootstrapperServiceType ServiceType => BootstrapperServiceType.Bus;

        public Action<BootstrappingContext> BootstrappAction
        {
            get; internal set;
        }

        #endregion

    }
}
