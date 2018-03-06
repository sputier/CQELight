using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.Attributes
{
    /// <summary>
    /// Attribute to define column name on a specific property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {

        #region Properties

        /// <summary>
        /// Name of the column into database.
        /// </summary>
        public string ColumnName { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create the attribute with a specific column name.
        /// </summary>
        /// <param name="columnName">Column's name.</param>
        public ColumnAttribute(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("ColumnAttribute.ctor() : Column's name should be provided.", nameof(columnName));
            }
            ColumnName = columnName;
        }

        #endregion

    }
}
