using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore
{
    class EFEventStoreBootstrappService : IBootstrapperService
    {

        #region IBoostrapperService

        public BootstrapperServiceType ServiceType => BootstrapperServiceType.EventStore;

        public Action BootstrappAction { get; internal set; }

        #endregion
    }
}
