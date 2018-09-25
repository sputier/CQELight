using System;
using System.Collections.Generic;
using CQELight;
using System.Text;

namespace CQELight.EventStore.MongoDb
{
    internal class MongoDbEventStoreBootstrappService : IBootstrapperService
    {
        #region IBootstrapperService

        public BootstrapperServiceType ServiceType => BootstrapperServiceType.EventStore;

        public Action<BootstrappingContext> BootstrappAction { get; internal set; }

        #endregion
    }
}
