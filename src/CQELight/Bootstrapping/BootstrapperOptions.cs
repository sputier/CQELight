using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight
{
    /// <summary>
    /// Bootstrapper starting options.
    /// </summary>
    public sealed class BootstrapperOptions
    {
        #region Properties

        /// <summary>
        /// Flag that indicates if MEF should be used for loading services.
        /// </summary>
        public bool UseMEF { get; set; }

        #endregion
    }
}
