using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagementService.Data
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> Get();
        void Add(Product product);
        void Edit(Product product);
        void Remove(Product product);
        Task<int> SaveAsync();
    }
}
