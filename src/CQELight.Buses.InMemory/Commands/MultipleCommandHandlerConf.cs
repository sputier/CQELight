using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Buses.InMemory.Commands
{
    /// <summary>
    /// Definition for multiple command handler.
    /// </summary>
    public class MultipleCommandHandlerConf
    {

        #region Properties
        /// <summary>
        /// Concerned type of the command.
        /// </summary>
        public Type CommandType { get; internal set; }
        /// <summary>
        /// Flag that indicates if handler should wait before going to the next one.
        /// </summary>
        public bool ShouldWait { get; internal set; }

        #endregion

        #region Ctor

        internal MultipleCommandHandlerConf() { }

        #endregion

    }
}
