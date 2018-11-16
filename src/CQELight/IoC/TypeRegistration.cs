using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.IoC
{
    /// <summary>
    /// Type for type registration.
    /// </summary>
    public class TypeRegistration : ITypeRegistration
    {
        #region Properties

        /// <summary>
        /// Type registrations.
        /// </summary>
        public IEnumerable<Type> AbstractionTypes { get; }
        /// <summary>
        /// Instance type.
        /// </summary>
        public Type InstanceType { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Create a new type as type(s) registration for IoC container.
        /// </summary>
        /// <param name="instanceType">Type instance.</param>
        /// <param name="registrationTypes">Registration types.</param>
        public TypeRegistration(Type instanceType, params Type[] registrationTypes)
        {
            InstanceType = instanceType ?? throw new ArgumentNullException(nameof(instanceType));
            AbstractionTypes = registrationTypes ?? throw new ArgumentNullException(nameof(registrationTypes));
            if (!registrationTypes.Any())
            {
                throw new ArgumentException("TypeRegistration.ctor() : You should provide at least one association type.");
            }
        }

        #endregion

    }
}
