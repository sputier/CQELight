using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Creates an instance of a type by reflection.
        /// </summary>
        /// <param name="type">Type of object expected.</param>
        /// <returns>Type instance.</returns>
        public static object CreateInstance(this Type type, params object[] parameters)
        {
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(m => m.GetParameters().Length == parameters.Length);
            if (ctor != null)
            {
                return ctor.Invoke(parameters);
            }
            return null;
        }


        /// <summary>
        /// Check if type implements at least one the generic interface.
        /// </summary>
        /// <param name="typeToCheck">Type to check.</param>
        /// <param name="genericInterfaceType">Interface's type.</param>
        /// <returns>VTrue if type implements it, false otherwise.</returns>
        public static bool ImplementsRawGenericInterface(this Type typeToCheck, Type genericInterfaceType)
            => typeToCheck.GetInterfaces().Any(m => m.IsGenericType && m.GetGenericTypeDefinition() == genericInterfaceType);

        #endregion

    }
}
