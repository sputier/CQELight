using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.IoC.Autofac
{
    internal class AutofacBootstrappService : IBootstrapperService
    {
        #region IBootstrapperService

        public BootstrapperServiceType ServiceType => BootstrapperServiceType.IoC;

        public Action BootstrappAction { get; internal set; }

        #endregion
    }
}
