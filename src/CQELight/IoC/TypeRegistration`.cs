using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.IoC
{
    /// <summary>
    /// Generic type registration.
    /// </summary>
    /// <typeparam name="T">Type of class to register.</typeparam>
    public class TypeRegistration<T> : ITypeRegistration
        where T : class
    {
        #region Members

        private readonly List<Type> _abstractionTypes = new List<Type>();

        #endregion

        #region Properties

        /// <summary>
        /// Type registrations.
        /// </summary>
        public IEnumerable<Type> AbstractionTypes { get => _abstractionTypes.AsEnumerable(); }

        /// <summary>
        /// Instance type.
        /// </summary>
        public Type InstanceType { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new type as type(s) registration for IoC container.
        /// </summary>
        /// <param name="registrationTypes">Registration types.</param>
        public TypeRegistration(params Type[] registrationTypes)
        {
            InstanceType = typeof(T);
            _abstractionTypes = registrationTypes?.ToList() ?? new List<Type>();
            _abstractionTypes.Add(InstanceType);
        }

        /// <summary>
        /// Create a new type as everything possible (self + all interfaces).
        /// </summary>
        /// <param name="forEverything">Flag that indicates if should register has everything possible.</param>
        public TypeRegistration(bool forEverything)
        {
            InstanceType = typeof(T);
            if(forEverything)
            {
                _abstractionTypes.Add(typeof(T));
                _abstractionTypes.AddRange(typeof(T).GetInterfaces());
            }
        }

        #endregion

    }
}
