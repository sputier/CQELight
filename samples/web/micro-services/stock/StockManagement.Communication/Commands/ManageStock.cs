using CQELight.Abstractions.CQS.Interfaces;
using SampleMicroservices.Common;
using System;

namespace StockManagement.Communication.Commands
{
    public sealed class ManageStock : ICommand
    {
        #region Properties

        public Action Action { get; private set; }

        public ProductId ProductId { get; private set; }

        #endregion

        #region Ctor

        public ManageStock(Action action, ProductId productId)
        {
            Action = action;
            ProductId = productId;
        }

        #endregion
    }
}