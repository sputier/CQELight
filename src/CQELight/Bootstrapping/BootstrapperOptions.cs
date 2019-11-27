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

        /// <summary>
        /// Flag to indicates if bootstrapper should stricly validates its content.
        /// </summary>
        public bool Strict { get; set; }

        /// <summary>
        /// Flag to indicates if optimal system is currently 'On', which means
        /// that one service of each kind should be provided.
        /// </summary>
        public bool CheckOptimal { get; set; }

        /// <summary>
        /// Flag to indicates if any encountered error notif on bootstraping
        /// should throw <see cref="BootstrappingException"/>
        /// </summary>
        public bool ThrowExceptionOnErrorNotif { get; set; }

        #endregion
    }
}
