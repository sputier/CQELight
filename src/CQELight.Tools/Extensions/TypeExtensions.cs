using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CQELight.Tools.Extensions
{
    /// <summary>
    /// Bunch of extensions methods for Type.
    /// </summary>
    public static class TypeExtensions
    {

        #region Public static methods

        /// <summary>
        /// Check if a type is in hierarchy of a parent type.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <param name="parent">Other to check if in hierarchy.</param>
        /// <returns>True if parent type or greater parent type, false otherwise.</returns>
        public static bool IsInHierarchySubClassOf(this Type type, Type parent)
        {
            if (type == parent || type.GetTypeInfo().IsSubclassOf(parent))
            {
                return true;
            }
            if (type == typeof(object) || type.GetTypeInfo().BaseType == null)
            {
                return false;
            }
            return type.GetTypeInfo().BaseType.IsInHierarchySubClassOf(parent);
        }

        #endregion

    }
}
