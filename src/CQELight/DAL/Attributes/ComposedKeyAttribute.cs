using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.DAL.Attributes
{
    /// <summary>
    /// Attribute to defined properties that holds composed key data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ComposedKeyAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// Name of properties that holds composed key data.
        /// </summary>
        public string[] PropertyNames { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Creation of the attribute.
        /// </summary>
        /// <param name="propertyNames">Name of properties of the composed key.</param>
        public ComposedKeyAttribute(params string[] propertyNames)
        {
            if (propertyNames == null || !propertyNames.Any())
            {
                throw new ArgumentNullException(nameof(propertyNames));
            }
            if (propertyNames.Length == 1)
            {
                throw new InvalidOperationException("ComposedKeyAttribute.Ctor() : Only one property has been set." +
                    " Use PrimaryKey attribute on top of it instead of this one.");
            }
            PropertyNames = propertyNames;
        }

        #endregion
    }
}
