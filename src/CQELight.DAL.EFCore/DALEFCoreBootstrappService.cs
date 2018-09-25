using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.EFCore
{
    internal class DALEFCoreBootstrappService : IBootstrapperService
    {
        #region IBoostrapperService

        public BootstrapperServiceType ServiceType => BootstrapperServiceType.DAL;

        public Action<BootstrappingContext> BootstrappAction { get; internal set; }

        #endregion
    }
}
