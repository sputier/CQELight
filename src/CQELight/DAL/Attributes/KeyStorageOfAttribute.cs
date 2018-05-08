using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.Attributes
{
    /// <summary>
    /// Attribute used to define the storage property for an object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class KeyStorageOfAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// Name of the property that represents the object.
        /// </summary>
        public string PropertyName { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Attribute used on top of a foreignkey property to define that this property holds the foreign key value of a specific object.
        /// </summary>
        /// <param name="propertyName">Name of the relation object.</param>
        public KeyStorageOfAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        #endregion
    }
}
