using AutoMapper;
using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.IoC.Interfaces;
using SampleMicroservices.Common;
using StockManagementService.Data;
using StockManagementService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagementService.Queries
{
    public interface IGetProductByIdQuery : IQuery<ProductInfo, ProductId> { }
    public class GetProductByIdQuery : IGetProductByIdQuery, IAutoRegisterType
    {
        #region Members

        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        #endregion

        #region Ctor

        public GetProductByIdQuery(
            IProductRepository productRepository,
            IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        #endregion

        #region IQuery methods

        public async Task<ProductInfo> ExecuteQueryAsync(ProductId param)
        {
            var dataProduct = (await _productRepository.Get()).FirstOrDefault(p => p.Id == param.Value);
            if (dataProduct != null)
            {
                return _mapper.Map<ProductInfo>(dataProduct);
            }
            return null;
        }

        #endregion
    }
}
