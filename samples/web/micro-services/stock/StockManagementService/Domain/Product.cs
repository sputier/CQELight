using CQELight.Abstractions.DDD;
using SampleMicroservices.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagementService.Domain
{
    class Product : Entity<ProductId>
    {
        #region Properties

        public string Name { get; set; }

        #endregion

        #region Ctor

        internal Product(ProductId id)
        {
            Id = id;
        }

        #endregion
    }
}
