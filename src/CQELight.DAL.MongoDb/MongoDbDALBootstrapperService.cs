using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.MongoDb
{
    class MongoDbDALBootstrapperService : IBootstrapperService
    {
        public BootstrapperServiceType ServiceType => BootstrapperServiceType.DAL;

        public Action<BootstrappingContext> BootstrappAction
        {
            get; internal set;
        }
    }
}
