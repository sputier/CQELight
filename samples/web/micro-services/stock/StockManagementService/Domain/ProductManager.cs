using CQELight.Abstractions.DDD;
using SampleMicroservices.Common;
using StockManagement.Communication.Events;
using StockManagementService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagementService.Domain
{
    public class ProductManager : AggregateRoot<Guid>
    {
        #region Members

        private List<Product> _products = new List<Product>();

        #endregion

        #region Public methods

        public Result AddProduct(ProductInfo product)
        {
            if (!_products.Any(p => p.Name == product.Name))
            {
                var productId = new ProductId(_products.Max(p => p.Id.Value) + 1);
                _products.Add(new Product(productId)
                {
                    Name = product.Name
                });
                AddDomainEvent(new StockManaged
                {
                    ProductId = productId,
                    Action = StockManagement.Communication.Action.Add
                });
                return Result.Ok(productId);
            }
            return Result.Fail("Product already exists");
        }

        #endregion
    }
}
