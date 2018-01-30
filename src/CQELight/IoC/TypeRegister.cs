using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Implementations.IoC
{
    /// <summary>
    /// Implementation of type register.
    /// </summary>
    public class TypeRegister : ITypeRegister
    {

        #region Members

        /// <summary>
        /// Collection of types to register as self and implemented interfaces.
        /// </summary>
        internal List<Type> _types = new List<Type>();
        /// <summary>
        /// Collection of objects to register as instances as self and implemented interfaces.
        /// </summary>
        internal List<object> _objects = new List<object>();
        /// <summary>
        /// Collection of objects to register a specific types.
        /// </summary>
        internal Dictionary<object, Type[]> _objAsTypes = new Dictionary<object, Type[]>();
        /// <summary>
        /// Collection of types to register as specific types.
        /// </summary>
        internal Dictionary<Type, Type[]> _typeAsTypes = new Dictionary<Type, Type[]>();


        #endregion

        #region Properties

        /// <summary>
        /// Collection of types to register as self and implemented interfaces.
        /// </summary>
        public IEnumerable<Type> Types => _types.AsEnumerable();
        /// <summary>
        /// Collection of objects to register as instances as self and implemented interfaces.
        /// </summary>
        public IEnumerable<object> Objects => _objects.AsEnumerable();
        /// <summary>
        /// Collection of objects to register a specific types.
        /// </summary>
        public IEnumerable<KeyValuePair<object, Type[]>> ObjAsTypes => _objAsTypes.AsEnumerable();
        /// <summary>
        /// Collection of types to register as specific types.
        /// </summary>
        public IEnumerable<KeyValuePair<Type, Type[]>> TypeAsTypes => _typeAsTypes.AsEnumerable();

        #endregion

        #region Public methods


        /// <summary>
        /// Register an object instance as itself and all its implementations.
        /// </summary>
        /// <param name="obj">Instance to register.</param>
        public void Register(object obj)
            => _objects.Add(obj);

        /// <summary>
        /// Register an object instance as specific types.
        /// </summary>
        /// <param name="obj">Instance to register.</param>
        /// <param name="types">List of types to register.</param>
        public void RegisterAs(object obj, params Type[] types)
            => _objAsTypes.Add(obj, types);

        /// <summary>
        /// Register a specific type as a collection of types.
        /// </summary>
        /// <typeparam name="T">Type to register.</typeparam>
        /// <param name="types">All types that will give an instance of T.</param>
        public void RegisterAs<T>(params Type[] types)
            => _typeAsTypes.Add(typeof(T), types);

        /// <summary>
        /// Register a specific type as itself and all implemented interfaces.
        /// </summary>
        /// <param name="type">Type to register.</param>
        public void RegisterType(Type type)
            => _types.Add(type);

        /// <summary>
        /// Register a specific type as itself and all implemented interfaces.
        /// </summary>
        /// <paramtype name="T">Type to register.</paramtype>
        public void RegisterType<T>()
            => _types.Add(typeof(T));

        #endregion
    }
}
