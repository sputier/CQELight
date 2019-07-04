using CQELight.Abstractions.DDD;
using SampleMicroservices.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AfterSaleService.Domain
{
    class AfterSaleCase : Entity<AfterSaleCaseId>
    {
        #region Properties

        public ProductId ConcernedProduct { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime? ClosingDate { get; set; }

        #endregion

        #region Ctor

        public AfterSaleCase(AfterSaleCaseId id)
        {
            Id = id;
        }

        #endregion
    }
}
