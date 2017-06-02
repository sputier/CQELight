using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.IoC.Interfaces
{
    /// <summary>
    /// Contract interface for factories of scope.
    /// </summary>
    public interface IScopeFactory
    {
        /// <summary>
        /// Create a new scope.
        /// </summary>
        /// <returns>New instance of scope.</returns>
        IScope CreateScope();
    }
}
