using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.Attributes
{
    /// <summary>
    /// Attribute used to specify that a specific column is indexed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IndexAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// Flag that indicates if index contains an unique clause.
        /// </summary>
        public bool IsUnique { get; private set; }
        /// <summary>
        /// Index name to be defined into database.
        /// </summary>
        public string IndexName { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create a new index.
        /// Unique is set to false by default and name is auto-generated.
        /// </summary>
        /// <param name="isUnique">Clause to indicates if index should contains an unique clause.</param>
        /// <param name="indexName">Name of the index.</param>
        public IndexAttribute(bool isUnique = false, string indexName = "")
        {
            IsUnique = isUnique;
            IndexName = indexName;
        }

        #endregion

    }
}
