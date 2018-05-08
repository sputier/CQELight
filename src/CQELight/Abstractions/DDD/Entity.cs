using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions
{
    /// <summary>
    /// Base entity class.
    /// </summary>
    /// <typeparam name="T">Type of the Id to use.</typeparam>
    public abstract class Entity<T>
    {
        #region Properties

        /// <summary>
        /// Unique Id of the Entity.
        /// </summary>
        public T Id { get; protected set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        protected Entity()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Redefining equality.
        /// </summary>
        /// <param name="obj">Other instance to compare with.</param>
        /// <returns>If both objects are equals.</returns>
        public override bool Equals(object obj)
        {
            if (!this.SameTypeCheck(obj))
                return false;
            return (obj as Entity<T>).Id.Equals(Id);
        }

        /// <summary>
        /// Getting hashcode of the object.
        /// </summary>
        /// <returns>Unique hashcode.</returns>
        public override int GetHashCode() => Id.ToString().GetHashCode();

        /// <summary>
        /// Override of equality operator
        /// </summary>
        /// <param name="obj1">First object to compare.</param>
        /// <param name="obj2">Second object to compare.</param>
        /// <returns>True if both have same Id, false otherwise.</returns>
        public static bool operator ==(Entity<T> obj1, Entity<T> obj2)
        {
            if (ReferenceEquals(obj1, null) && ReferenceEquals(obj2, null))
                return true;

            if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Override of inequality operator.
        /// </summary>
        /// <param name="obj1">First object to compare.</param>
        /// <param name="obj2">Second object to compare.</param>
        /// <returns>True if both don't have same Id, false otherwise.</returns>
        public static bool operator !=(Entity<T> obj1, Entity<T> obj2) => !(obj1 == obj2);

        #endregion

    }
}
