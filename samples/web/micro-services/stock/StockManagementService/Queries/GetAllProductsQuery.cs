using AutoMapper;
using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using StockManagementService.Data;
using StockManagementService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagementService.Queries
{
    public interface IGetAllProductsQuery : IQuery<IEnumerable<ProductInfo>> { }
    class GetAllProductsQuery : IGetAllProductsQuery, IAutoRegisterType
    {
        #region Members

        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        #endregion

        #region Ctor

        public GetAllProductsQuery(
            IProductRepository productRepository,
            IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        #endregion
        public async Task<IEnumerable<ProductInfo>> ExecuteQueryAsync()
        {
            var allProduct = await _productRepository.Get();
            return allProduct.Select(p => _mapper.Map<ProductInfo>(p)).ToList();
        }
    }
}
