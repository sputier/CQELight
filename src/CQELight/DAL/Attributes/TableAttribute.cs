using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.Attributes
{
    /// <summary>
    /// Attribute to define a table into database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// Name of the table.
        /// </summary>
        public string TableName { get; private set; }
        /// <summary>
        /// Schema name.
        /// </summary>
        public string SchemaName { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creation of the attribute.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="schemaName">Name of the schema</param>
        public TableAttribute(string tableName = "", string schemaName = "dbo")
        {
            TableName = tableName;
            SchemaName = schemaName;
        }

        #endregion
    }
}
