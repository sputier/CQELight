using CQELight.Abstractions.Events;
using SampleMicroservices.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockManagement.Communication.Events
{
    public sealed class StockManaged : BaseDomainEvent
    {
        #region Properties

        public ProductId ProductId { get; internal set; }
        public Action Action { get; internal set; }

        #endregion

        #region Ctor

        internal StockManaged() { }

        #endregion
    }
}
