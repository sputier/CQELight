using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.MongoDb
{
    internal class MongoDbEventStoreBootstrappService : IBootstrapperService
    {
        #region IBootstrapperService

        public BootstrapperServiceType ServiceType => BootstrapperServiceType.EventStore;

        public Action BootstrappAction { get; internal set; }

        #endregion
    }
}
