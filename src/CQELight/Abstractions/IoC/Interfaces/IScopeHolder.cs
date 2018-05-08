using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.IoC.Interfaces
{
    /// <summary>
    /// Contract interface for class that holds a instance of a scope.
    /// </summary>
    public interface IScopeHolder
    {
        /// <summary>
        /// Instance of scope currently holding.
        /// </summary>
        IScope Scope { get; }
    }
}
