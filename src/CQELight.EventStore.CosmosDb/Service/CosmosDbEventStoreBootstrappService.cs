using System;

namespace CQELight.EventStore.CosmosDb.Service
{
    internal class CosmosDbEventStoreBootstrappService : IBootstrapperService
    {
        #region IBootstrapperService

        public BootstrapperServiceType ServiceType => BootstrapperServiceType.EventStore;

        public Action<BootstrappingContext> BootstrappAction { get; internal set; }

        #endregion
    }
}
