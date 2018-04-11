using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.EFCore
{
    class DALEFCoreBootstrappService : IBootstrapperService
    {

        #region IBoostrapperService

        public BootstrapperServiceType ServiceType => BootstrapperServiceType.DAL;

        public Action BootstrappAction { get; internal set; }

        #endregion
    }
}
