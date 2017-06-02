using CQELight.Abstractions.IoC.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.IoC
{
    /// <summary>
    /// Parameter for resolving by name.
    /// </summary>
    public class NameResolverParameter : IResolverParameter
    {

        #region Properties

        /// <summary>
        /// Name of parameter for resolution.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Value of parameter.
        /// </summary>
        public object Value { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Constructor of parameter.
        /// </summary>
        /// <param name="name">Name of parameter.</param>
        /// <param name="value">Value of parameter.</param>
        public NameResolverParameter(string name, object value)
        {
            Value = value;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        #endregion

    }
}
