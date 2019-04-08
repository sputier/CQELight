using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Tools
{
    /// <summary>
    /// EqualityComparer for types to compare on assemblyQualifiedName.
    /// </summary>
    public class TypeEqualityComparer : IEqualityComparer<Type>
    {
        #region IEqualityComparer

        /// <summary>Determines whether the specified types are equal.</summary>
        /// <param name="x">The first type to compare.</param>
        /// <param name="y">The second type to compare.</param>
        /// <returns>true if the specified types are equal; otherwise, false.</returns>
        public bool Equals(Type x, Type y)
            => x.AssemblyQualifiedName == y.AssemblyQualifiedName;

        /// <summary>Returns a hash code for the specified type.</summary>
        /// <param name="obj">The <see cref="object"></see> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="ArgumentNullException">The type of <paramref name="obj">obj</paramref> is a reference type and <paramref name="obj">obj</paramref> is null.</exception>
        public int GetHashCode(Type obj)
            => obj.AssemblyQualifiedName.GetHashCode();

        #endregion
    }
}
