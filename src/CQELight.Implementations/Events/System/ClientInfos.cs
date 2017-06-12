using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Implementations.Events.System
{
    /// <summary>
    /// Small model to handle client infos in system bus.
    /// </summary>
    public class ClientInfos
    {

        #region Properties

        /// <summary>
        /// Name of connected client.
        /// </summary>
        public string ClientName { get; set; }
        /// <summary>
        /// Id of connected client.
        /// </summary>
        public Guid ClientID { get; set; }

        #endregion

    }
}
