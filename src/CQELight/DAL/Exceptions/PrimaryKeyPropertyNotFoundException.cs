using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CQELight.DAL.Exceptions
{
#pragma warning disable RCS1194, S3925
    /// <summary>
    /// Specific exception to indicates that property that holds primary key hasn't been found.
    /// </summary>
    public class PrimaryKeyPropertyNotFoundException : Exception
#pragma warning restore RCS1194, S3925
    {

        #region Properties
        /// <summary>
        /// Type of entity.
        /// </summary>
        public Type EntityType { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Creation of a new PrimaryKeyPropertyNotFound exception.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        public PrimaryKeyPropertyNotFoundException( Type entityType)
            : base($"No primary key property hasn't been found on type {entityType.FullName}.")
        {
            EntityType = entityType;
        }

        #endregion

    }
}