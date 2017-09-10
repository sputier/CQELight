using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Implementations.IoC
{
    /// <summary>
    /// Internal implémentation of type register.
    /// </summary>
    class TypeRegister : ITypeRegister
    {

        #region Properties

        /// <summary>
        /// Collection of types to register as self and implemented interfaces.
        /// </summary>
        internal List<Type> Types { get; set; } = new List<Type>();
        /// <summary>
        /// Collection of objects to register as instances as self and implemented interfaces.
        /// </summary>
        internal List<object> Objects { get; set; } = new List<object>();
        /// <summary>
        /// Collection of objects to register a specific types.
        /// </summary>
        internal Dictionary<object, Type[]> ObjAsTypes { get; set; } = new Dictionary<object, Type[]>();
        /// <summary>
        /// Collection of types to register as specific types.
        /// </summary>
        internal Dictionary<Type, Type[]> TypeAsTypes { get; set; } = new Dictionary<Type, Type[]>();

        #endregion

        #region Public methods


        /// <summary>
        /// Register an object instance as itself and all its implementations.
        /// </summary>
        /// <param name="obj">Instance to register.</param>
        public void Register(object obj)
            => Objects.Add(obj);
        
        /// <summary>
        /// Register an object instance as specific types.
        /// </summary>
        /// <param name="obj">Instance to register.</param>
        /// <param name="types">List of types to register.</param>
        public void RegisterAs(object obj, params Type[] types)
            => ObjAsTypes.Add(obj, types);
        
        /// <summary>
        /// Register a specific type as a collection of types.
        /// </summary>
        /// <typeparam name="T">Type to register.</typeparam>
        /// <param name="types">All types that will give an instance of T.</param>
        public void RegisterAs<T>(params Type[] types)
            => TypeAsTypes.Add(typeof(T), types);
        
        /// <summary>
        /// Register a specific type as itself and all implemented interfaces.
        /// </summary>
        /// <param name="type">Type to register.</param>
        public void RegisterType(Type type)
            => Types.Add(type);
        
        /// <summary>
        /// Register a specific type as itself and all implemented interfaces.
        /// </summary>
        /// <paramtype name="T">Type to register.</paramtype>
        public void RegisterType<T>()
            => Types.Add(typeof(T));

        #endregion
    }
}
