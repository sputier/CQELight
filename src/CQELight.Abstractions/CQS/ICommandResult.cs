using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions
{
    /// <summary>
    /// Contract interface for command Result.
    /// </summary>
    public interface ICommandResult
    {
        /// <summary>
        /// Result of command execution.
        /// </summary>
        bool Result { get; }

    }
}
