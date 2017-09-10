using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Implementations.IoC
{
    /// <summary>
    /// Typed parameter resolver.
    /// </summary>
    public class TypeResolverParameter : IResolverParameter
    {

        #region Properties

        /// <summary>
        /// Type of the parameter
        /// </summary>
        public Type Type { get; }
        /// <summary>
        /// Value of the parameter
        /// </summary>
        public object Value { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="type">Parameter's type.</param>
        /// <param name="value">Parameter's value.</param>
        public TypeResolverParameter(Type type, object value)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Value = value;
        }

        #endregion

    }
}
