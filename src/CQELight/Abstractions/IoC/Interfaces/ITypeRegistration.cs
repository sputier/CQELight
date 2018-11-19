using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.IoC.Interfaces
{
    /// <summary>
    /// Contract interface for type registration into Ioc container.
    /// </summary>
    public interface ITypeRegistration
    {
        /// <summary>
        /// Type of abstraction that is concerned by this registration.
        /// </summary>
        IEnumerable<Type> AbstractionTypes { get; }
    }
}
