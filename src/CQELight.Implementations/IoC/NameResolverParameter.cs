using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Implementations.IoC
{
    /// <summary>
    /// Named parameter resolver.
    /// </summary>
    public class NameResolverParameter : IResolverParameter
    {

        #region Properties

        /// <summary>
        /// Name of the parameter.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Value of the parameter.
        /// </summary>
        public object Value { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        /// <param name="name">Parameter's name.</param>
        /// <param name="value">Parmaeter's value.</param>
        public NameResolverParameter(string name, object value)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            Value = value;
            Name = name;
        }

        #endregion

    }
}
