using Autofac.Core.Activators.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQELight.IoC.Autofac
{
    /// <summary>
    /// Class to help Autofac finding every constructor, even those who are not public.
    /// </summary>
    public class FullConstructorFinder : IConstructorFinder
    {

        #region Members

        static readonly ConcurrentDictionary<Type, ConstructorInfo[]> _defaultPublicConstructorsCache
            = new ConcurrentDictionary<Type, ConstructorInfo[]>();

        #endregion

        #region IConstructorFinder

        /// <summary>
        /// Get all constructor that can be instantiated.
        /// </summary>
        /// <param name="targetType">Target type.</param>
        /// <returns>Array of available constructors.</returns>
        public ConstructorInfo[] FindConstructors(Type targetType)
            =>
            _defaultPublicConstructorsCache.GetOrAdd(targetType, t => t.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic).ToArray());


        #endregion
    }
}
