using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Infrastructure
{
    /// <summary>
    /// Type resolver contract interface.
    /// </summary>
    public interface ITypeResolver
    {
        /// <summary>
        /// Resolve an instance of type T.
        /// </summary>
        /// <typeparam name="T">Type to resolve.</typeparam>
        /// <returns>Instance of T</returns>
        T Resolve<T>(params TypeResolverParameter[] parameters);
        /// <summary>
        /// Resolve an instance of asked type.
        /// </summary>
        /// <param name="type">Type to resolve.</param>
        /// <returns>Instance of type</returns>
        object Resolve(Type type, params TypeResolverParameter[] parameters);
    }
}
