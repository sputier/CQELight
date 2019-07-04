using CQELight.Abstractions.Events;
using SampleMicroservices.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSale.Communication.Events
{
    public sealed class AfterSaleCaseResolved : BaseDomainEvent
    {
        #region Properties

        public ProductId ProductId { get; internal set; }
        public AfterSaleCaseId CaseId { get; internal set; }

        #endregion

        #region Ctor

        internal AfterSaleCaseResolved() { }

        #endregion
    }
}
