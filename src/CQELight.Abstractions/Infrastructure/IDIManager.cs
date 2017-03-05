using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Infrastructure
{
    /// <summary>
    /// Contract interface for dependency injection.
    /// </summary>
    public interface IDIManager
    {
        /// <summary>
        /// Register an abstract type as an implementation Type.
        /// </summary>
        /// <param name="abstractType">Abstract type.</param>
        /// <param name="implementationType">Implementation type.</param>
        void RegisterAs(Type abstractType, Type implementationType);


    }
}
