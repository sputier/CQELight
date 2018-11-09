using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore
{
    internal class EFEventStoreBootstrappService : IBootstrapperService
    {
        #region IBoostrapperService

        public BootstrapperServiceType ServiceType => BootstrapperServiceType.EventStore;

        public Action<BootstrappingContext> BootstrappAction { get; internal set; }

        #endregion
    }
}
