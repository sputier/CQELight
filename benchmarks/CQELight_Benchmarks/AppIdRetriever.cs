using CQELight.Abstractions.Configuration;
using CQELight.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight_Benchmarks
{
    class AppIdRetriever : IAppIdRetriever
    {

        #region IAppIdRetriever

        public AppId GetAppId()
            => new AppId(Guid.Parse("A0165D77-E5C4-4B9B-A0D5-002163F477C0"));

        #endregion
    }
}
