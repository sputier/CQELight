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

        /// <summary>
        /// Cache de récupération de constructeurs.
        /// </summary>
        static readonly ConcurrentDictionary<Type, ConstructorInfo[]> _defaultPublicConstructorsCache
            = new ConcurrentDictionary<Type, ConstructorInfo[]>();

        #endregion

        #region IConstructorFinder

        /// <summary>
        /// Récupération des constructeurs éligibles.
        /// </summary>
        /// <param name="targetType">Type dont on veut les constructeurs.</param>
        /// <returns>Liste des constructeurs.</returns>
        public ConstructorInfo[] FindConstructors(Type targetType)
            =>
            _defaultPublicConstructorsCache.GetOrAdd(targetType, t => t.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic).ToArray());


        #endregion
    }
}
