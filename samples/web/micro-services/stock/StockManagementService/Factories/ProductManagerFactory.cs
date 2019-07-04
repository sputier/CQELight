using CQELight.Abstractions.IoC.Interfaces;
using StockManagementService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagementService.Factories
{
    public class ProductManagerFactory : IAutoRegisterTypeSingleInstance
    {
        #region Members

        private ProductManager _productManager;

        #endregion

        #region Public methods

        public ProductManager GetManager()
        {
            if (_productManager == null)
            {
                _productManager = new ProductManager();
            }
            return _productManager;
        }

        #endregion
    }
}
