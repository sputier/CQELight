using AfterSaleService.Factories;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using StockManagement.Communication.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AfterSaleService.Handlers
{
    public class StockManagedHandler : IDomainEventHandler<StockManaged>
    {
        #region Members

        private readonly AfterSaleManagerFactory _afterSaleManagerFactory;

        #endregion

        #region Ctor

        public StockManagedHandler(
            AfterSaleManagerFactory afterSaleManagerFactory)
        {
            _afterSaleManagerFactory = afterSaleManagerFactory ?? throw new ArgumentNullException(nameof(afterSaleManagerFactory));
        }

        #endregion

        #region Public methods

        public async Task<Result> HandleAsync(StockManaged domainEvent, IEventContext context = null)
        {
            if (domainEvent.Action == StockManagement.Communication.Action.Add)
            {
                var manager = _afterSaleManagerFactory.GetManager();
                var result = manager.ResolveCaseForProductId(domainEvent.ProductId);
                if (result)
                {
                    await manager.PublishDomainEventsAsync();
                }
            }
            return Result.Ok();
        }

        #endregion
    }
}
