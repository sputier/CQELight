using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.GregYoungsEventStore
{
    internal class GregYoungsEventStoreBootstrappService
    {
        #region IBoostrapperService

        public BootstrapperServiceType ServiceType => BootstrapperServiceType.EventStore;

        public Action<BootstrappingContext> BootstrappAction { get; internal set; }

        #endregion
    }
}
