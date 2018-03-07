using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.DAL.Attributes
{
    /// <summary>
    /// Attribute to define an index on multiple columns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ComplexIndexAttribute : Attribute
    {

        #region Properties

        /// <summary>
        /// Flag to indicate if index has an unique clause.
        /// </summary>
        public bool IsUnique { get; private set; }

        /// <summary>
        /// Name of all properties that are parts of the index.
        /// </summary>
        public string[] PropertyNames { get; private set; }

        /// <summary>
        /// Name of the index.
        /// </summary>
        public string IndexName { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create a complex index attribute on the specified properties.
        /// Index will not have unique clause and name will be auto generated.
        /// </summary>
        /// <param name="propertyNames">Properties that are part of the index.</param>
        public ComplexIndexAttribute(params string[] propertyNames)
            : this(propertyNames, false, "")
        {
        }

        /// <summary>
        /// Creates a new complex index attribute upon a specific class,
        /// on the specific properties, with a unique clause.
        /// Index name will be auto generated.
        /// </summary>
        /// <param name="propertyNames">Properties that are part of the index.</param>
        /// <param name="isUnique">Flag that indicates if index is unique.</param>
        public ComplexIndexAttribute(string[] propertyNames, bool isUnique)
            : this(propertyNames, isUnique, "")
        {
        }

        /// <summary>
        /// Creates a new complex index attribute upon a specific class,
        /// on the specific properties, with a unique clause and a specific name.
        /// </summary>
        /// <param name="propertyNames">Properties that are part of the index.</param>
        /// <param name="isUnique">Flag that indicates if index is unique.</param>
        /// <param name="idxName">Name that the index should have.</param>
        public ComplexIndexAttribute(string[] propertyNames, bool isUnique, string idxName)
        {
            if(propertyNames == null )
            {
                throw new ArgumentNullException(nameof(propertyNames));
            }
            if(!propertyNames.Any() || !propertyNames.Skip(1).Any())
            {
                throw new ArgumentException("ComplexIndexAttribute.Ctor() : Properties should be provived and greater than 1. " +
                    "If only one, you should use Index attribute on the specified property.", nameof(propertyNames));
            }
            PropertyNames = propertyNames;
            IsUnique = isUnique;
            IndexName = idxName;
        }

        #endregion

    }
}
