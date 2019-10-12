using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight
{
    /// <summary>
    /// Bootstrapper options.
    /// </summary>
    public sealed class BootstrapperOptions
    {
        #region Properties

        /// <summary>
        /// Flag that indicates if services are autoloaded.
        /// Warning : Setting this flag to true will ignore all your custom calls.
        /// </summary>
        public bool AutoLoad { get; set; }

        #endregion
    }
}
