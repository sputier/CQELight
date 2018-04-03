using CQELight.Abstractions.Dispatcher.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Dispatcher.Configuration.Internal
{
    /// <summary>
    /// Internal class to help managing configuration to build.
    /// </summary>
    internal class CommandDispatchConfiguration : BaseDispatchConfiguration
    {

        #region Properties

        /// <summary>
        /// Type of command configuration is about.
        /// </summary>
        public Type CommandType { get; set; }

        #endregion

    }
}
