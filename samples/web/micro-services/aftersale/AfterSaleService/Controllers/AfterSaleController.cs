using AfterSaleService.Domain;
using AfterSaleService.Factories;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Dispatcher.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SampleMicroservices.Common;
using StockManagement.Communication.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AfterSaleService.Controllers
{
    public class AfterSaleController : Controller
    {
        #region Members

        private readonly IDispatcher _dispatcher;
        private readonly AfterSaleManagerFactory _afterSaleManagerFactory;

        #endregion

        #region Ctor

        public AfterSaleController(
            AfterSaleManagerFactory afterSaleManagerFactory,
            IDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _afterSaleManagerFactory = afterSaleManagerFactory ?? throw new ArgumentNullException(nameof(afterSaleManagerFactory));
        }

        #endregion

        #region Public methods

        #region GET

        #endregion

        #region POST

        [HttpPost]
        public async Task<IActionResult> ResolveCase(AfterSaleCaseId id)
        {
            var manager = _afterSaleManagerFactory.GetManager();
            var result = manager.ResolveAfterSaleCase(id);
            if (result)
            {
                var productId = manager.GetCaseProductId(id);
                await _dispatcher.DispatchCommandAsync(new ManageStock(StockManagement.Communication.Action.Remove, productId.Value));
                return Ok();
            }
            return UnprocessableEntity((result as Result<string>).Value);
        }

        #endregion

        #endregion
    }
}
