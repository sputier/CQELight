using CQELight.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Configuration
{
    /// <summary>
    /// Contract interface for getting AppId of current app.
    /// </summary>
    public interface IAppIdRetriever
    {

        /// <summary>
        /// Retrieve current AppId.
        /// </summary>
        /// <returns>Current AppId</returns>
        AppId GetAppId();

    }
}
