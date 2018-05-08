using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.Exceptions
{
    /// <summary>
    /// Specific exception to handle case when composed key attribute was not defined.
    /// </summary>
#pragma warning disable RCS1194, S3925
    public class ComposedKeyAttributeNotDefinedException : Exception
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
        public ComposedKeyAttributeNotDefinedException(Type entityType)
            : base($"No composed key attribute has been define on type {entityType.FullName}.")
        {
            EntityType = entityType;
        }

        #endregion

    }
}
