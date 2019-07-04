using AfterSale.Communication.Events;
using CQELight.Abstractions.DDD;
using SampleMicroservices.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AfterSaleService.Domain
{
    public class AfterSaleManager : AggregateRoot<Guid>
    {
        #region Members

        private List<AfterSaleCase> _cases = new List<AfterSaleCase>();

        #endregion

        #region Public methodss

        public ProductId? GetCaseProductId(AfterSaleCaseId id)
            => _cases.FirstOrDefault(c => c.Id.Value == id.Value)?.ConcernedProduct;

        public Result ResolveAfterSaleCase(AfterSaleCaseId id)
        {
            var afterSaleCase = _cases.FirstOrDefault(c => c.Id.Value == id.Value);
            if (afterSaleCase != null)
            {
                afterSaleCase.ClosingDate = DateTime.Now;
                AddDomainEvent(new AfterSaleCaseResolved
                {
                    CaseId = id,
                    ProductId = afterSaleCase.ConcernedProduct
                });
                return Result.Ok();
            }
            return Result.Fail($"Case {id.Value} is not found.");
        }

        public Result ResolveCaseForProductId(ProductId id)
        {
            var afterSaleCase =
                _cases
                    .Where(c => !c.ClosingDate.HasValue)
                    .OrderBy(c => c.OpeningDate)
                    .FirstOrDefault(c => c.ConcernedProduct.Value == id.Value);
            if (afterSaleCase != null)
            {
                return ResolveAfterSaleCase(afterSaleCase.Id);
            }
            return Result.Fail();
        }

        #endregion
    }
}
