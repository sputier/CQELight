using AutoMapper;
using CQELight.Abstractions.DDD;
using Microsoft.AspNetCore.Mvc;
using SampleMicroservices.Common;
using StockManagementService.Data;
using StockManagementService.Factories;
using StockManagementService.Models;
using StockManagementService.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagementService.Controllers
{
    [ApiController]
    [Route("api/v1/products/[action]")]
    public class ProductManagementController : Controller
    {
        #region Members

        private readonly IGetAllProductsQuery _productQuery;
        private readonly ProductManagerFactory _productManagerFactory;
        private readonly IMapper _mapper;
        private readonly IGetProductByIdQuery _productByIdQuery;

        #endregion

        #region Ctor

        public ProductManagementController(
            IGetAllProductsQuery productQuery,
            IGetProductByIdQuery productByIdQuery,
            ProductManagerFactory productManagerFactory,
            IMapper mapper)
        {
            _productQuery = productQuery ?? throw new ArgumentNullException(nameof(productQuery));
            _productManagerFactory = productManagerFactory;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _productByIdQuery = productByIdQuery ?? throw new ArgumentNullException(nameof(productByIdQuery));
        }

        #endregion

        #region Public methods

        #region GET

        [HttpGet]
        public async Task<IActionResult> Get()
            => Ok(await _productQuery.ExecuteQueryAsync());

        [HttpGet]
        public async Task<IActionResult> GetById(ProductId id)
        {
            var product = await _productByIdQuery.ExecuteQueryAsync(id);
            if(product != null)
            {
                return Ok(product);
            }
            return NotFound();
        }

        #endregion

        #region POST

        public async Task<IActionResult> CreateNew(ProductInfo infos)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var manager = _productManagerFactory.GetManager();
            var result = manager.AddProduct(infos);
            if (result)
            { 
                await manager.PublishDomainEventsAsync();
                return CreatedAtAction(nameof(ProductManagementController.GetById),
                    (result as Result<ProductId>).Value);
            }
            return BadRequest((result as Result<string>).Value);
        }

        #endregion

        #endregion
    }
}
