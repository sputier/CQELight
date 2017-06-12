using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Implementations
{
    /// <summary>
    /// Consts class.
    /// </summary>
    static class Consts
    {

        /// <summary>
        /// Chaine de connexion.
        /// </summary>
        public const string CONST_CONNECTION_STRING_LOCALDB =
#if(DEBUG)
            "Server=(localdb)\\mssqllocaldb;Database=Events_BDD;Trusted_Connection=True;MultipleActiveResultSets=true"
#else
            ""
#endif
            ;

        #region SystemBus

        /// <summary>
        /// Authkey to communicate with system bus.
        /// </summary>
        // Guid bdd02ac6-600f-4bb0-9245-219d4af7e870 in MD5
        public static readonly string CONST_SYSTEM_BUS_AUTH_KEY = "ff3ea6891e377fee8e4700da5e710837";
        /// <summary>
        /// Token to mark message as well received.
        /// </summary>
        public static readonly string CONST_SYSTEM_BUS_WELL_RECEIVED_TOKEN = "message_ok";
        /// <summary>
        /// Name of the authentification pipe.
        /// </summary>
        public static readonly string CONST_SYSTEM_BUS_AUTH_PIPE_NAME = CONST_SYSTEM_BUS_DEDICATED_PIPE_PREFIX + "Auth";
        /// <summary>
        /// Prefix for pipe to communicate with system bus.
        /// </summary>
        public static readonly string CONST_SYSTEM_BUS_DEDICATED_PIPE_PREFIX = "KernelCore.SystemBus.";
        /// <summary>
        /// Indicates if system bus is ready.
        /// </summary>
        public static readonly string CONST_SYSTEM_BUS_READY = "SystemBus.Ready";
        /// <summary>
        /// Prefix for pipe as server to receive events from system bus.
        /// </summary>
        public static readonly string CONST_SYSTEM_BUS_SYSTEM_EVENT_BUS_SERVER_NAME = "KernelCore.SystemEventBus.";

        #endregion

    }
}
