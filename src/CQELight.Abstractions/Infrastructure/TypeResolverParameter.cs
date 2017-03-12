using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.Infrastructure
{
    /// <summary>
    /// Parameter class for resolving.
    /// </summary>
    public class TypeResolverParameter
    {

        #region Properties
        /// <summary>
        /// Value of the parameter.
        /// </summary>
        public object Value { get; protected set; }
        /// <summary>
        /// Name of the paramater.
        /// </summary>
        public string Name { get; protected set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Default constructor for parameteR.
        /// </summary>
        /// <param name="name">Name of the parameter for resolving.</param>
        /// <param name="value">Value of the parameter for resolving.</param>
        public TypeResolverParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        #endregion

    }
}
