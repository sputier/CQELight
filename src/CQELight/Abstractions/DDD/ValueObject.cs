using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions.DDD
{
    /// <summary>
    /// Definition of a Value objecT.
    /// </summary>
    /// <typeparam name="T">Type of Value Object.</typeparam>
    public abstract class ValueObject<T> where T : ValueObject<T>
    {

        #region Public methods

        /// <summary>
        /// Redifining equality.
        /// </summary>
        /// <param name="obj">Other value object to compare.</param>
        /// <returns>If both value objects are equals.</returns>
        public override bool Equals(object obj)
        {
            var valueObject = obj as T;

            if (ReferenceEquals(valueObject, null))
                return false;

            return EqualsCore(valueObject);
        }

        /// <summary>
        /// Get the unique hashcode of the object.
        /// </summary>
        /// <returns>Hashcode.</returns>
        public override int GetHashCode() => GetHashCodeCore();


        #endregion

        #region Operators

        /// <summary>
        /// Override of equality operator
        /// </summary>
        /// <param name="obj1">First object to compare.</param>
        /// <param name="obj2">Second object to compare.</param>
        /// <returns>True if both are same, false otherwise.</returns>
        public static bool operator ==(ValueObject<T> obj1, ValueObject<T> obj2)
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
        /// <returns>True if both aren't same, false otherwise.</returns>
        public static bool operator !=(ValueObject<T> obj1, ValueObject<T> obj2) => !(obj1 == obj2);

        #endregion

        #region Protected methods

        /// <summary>
        /// Custom implementation of equality of ValueObject. This should be inherited and redefined within every VO to implements business logic.
        /// </summary>
        /// <param name="other">Other instance of ValueObject to compare with.</param>
        /// <returns>True if equals, false otherwise.</returns>
        protected abstract bool EqualsCore(T other);

        /// <summary>
        /// Custom implementation of Hashcode of ValueObject. 
        /// </summary>
        /// <returns>Hashcode of the instance.</returns>
        protected abstract int GetHashCodeCore();

        #endregion
    }
}
