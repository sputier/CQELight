using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.IoC.Interfaces
{
    /// <summary>
    /// Contract interface to help generalize registration.
    /// </summary>
    public interface ITypeRegister
    {
        /// <summary>
        /// Register an object instance as itself and all its implementations.
        /// </summary>
        /// <param name="obj">Instance to register.</param>
        void Register(object obj);
        /// <summary>
        /// Register an object instance as specific types.
        /// </summary>
        /// <param name="obj">Instance to register.</param>
        /// <param name="types">List of types to register.</param>
        void RegisterAs(object obj, params Type[] types);
        /// <summary>
        /// Register a specific type as itself and all implemented interfaces.
        /// </summary>
        /// <param name="type">Type to register.</param>
        void RegisterType(Type type);
        /// <summary>
        /// Register a specific type as itself and all implemented interfaces.
        /// </summary>
        /// <paramtype name="T">Type to register.</paramtype>
        void RegisterType<T>();
        /// <summary>
        /// Register a specific type as a collection of types.
        /// </summary>
        /// <typeparam name="T">Type to register.</typeparam>
        /// <param name="types">All types that will give an instance of T.</param>
        void RegisterAs<T>(params Type[] types);
    }
}
