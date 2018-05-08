using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.Attributes
{
    /// <summary>
    /// Attribute to define ForeignKey of a relation object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ForeignKeyAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// Name of property that maps for ForeignKey. This can be used if you have a self parent-children relation, or multiples one to many
        /// relations.
        /// </summary>
        public string InversePropertyName { get; set; }
        /// <summary>
        /// Flag that indicates if DeleteCascade should be applied.s
        /// </summary>
        public bool DeleteCascade { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Defining a relationship with a Foreign Key, and setting up some configuration upon it.
        /// </summary>
        /// <param name="inversePropertyName">Name of distant property.</param>
        /// <param name="deleteCascade">Apply delete cascade when deleting parent object..</param>
        public ForeignKeyAttribute(string inversePropertyName = "", bool deleteCascade = false)
        {
            InversePropertyName = inversePropertyName;
            DeleteCascade = deleteCascade;
        }

        #endregion

    }
}
