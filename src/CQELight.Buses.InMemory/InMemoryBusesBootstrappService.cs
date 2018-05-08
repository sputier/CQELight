using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.InMemory
{
    internal class InMemoryBusesBootstrappService : IBootstrapperService
    {
        #region Static members

        private static InMemoryBusesBootstrappService _instance;

        internal static InMemoryBusesBootstrappService Instance
        {
            get
            {
                if(_instance ==null)
                {
                    _instance = new InMemoryBusesBootstrappService();
                }
                return _instance;
            }
        }

        #endregion

        #region IBootstrapperService

        public BootstrapperServiceType ServiceType => BootstrapperServiceType.Bus;

        public Action BootstrappAction { get; internal set; } = () => { };

        #endregion

    }
}
