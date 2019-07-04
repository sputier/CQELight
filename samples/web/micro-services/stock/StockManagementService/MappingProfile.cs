using AutoMapper;
using StockManagementService.Data;
using StockManagementService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagementService
{
    public class StockMappingProfile : Profile
    {
        #region Ctor

        public StockMappingProfile()
        {
            CreateMap<Product, ProductInfo>();
            CreateMap<ProductInfo, Product>();
        }

        #endregion
    }
}
