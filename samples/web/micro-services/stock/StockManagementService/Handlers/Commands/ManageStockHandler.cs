using CQELight.Abstractions.CQS.Interfaces;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.IoC.Interfaces;
using StockManagement.Communication.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagementService.Handlers.Commands
{
    public class ManageStockHandler : ICommandHandler<ManageStock>, IAutoRegisterType
    {
        #region Members

        #endregion

        #region Ctor

        public ManageStockHandler()
        {

        }

        #endregion

        #region ICommandHandler methods

        public Task<Result> HandleAsync(ManageStock command, ICommandContext context = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
