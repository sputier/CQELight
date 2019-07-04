using AfterSaleService.Domain;
using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AfterSaleService.Factories
{
    public class AfterSaleManagerFactory : IAutoRegisterTypeSingleInstance
    {
        #region Members

        private AfterSaleManager _manager;

        #endregion

        #region Public methods

        public AfterSaleManager GetManager()
        {
            if(_manager == null)
            {
                _manager = new AfterSaleManager();
            }
            return _manager;
        }

        #endregion
    }
}
