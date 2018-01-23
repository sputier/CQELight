using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQELight.Tools.Extensions
{
    /// <summary>
    /// Bunch of extensions methods for collections.
    /// </summary>
    public static class CollectionExtensions
    {

        #region Public static methods

        /// <summary>
        /// Do an action on each member of a enumerable collection.
        /// </summary>
        /// <typeparam name="T">Type of enumerable collection objectS.</typeparam>
        /// <param name="collection">Instance of the collection.</param>
        /// <param name="action">Aaction to perform</param>
        public static void DoForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (action == null)
                return;
            foreach (var item in collection)
            {
                action(item);
            }
        }

        /// <summary>
        /// Get a collection filtered where any element is not equals to default value (such as null for objects).
        /// </summary>
        /// <param name="collection">Collection to filter.</param>
        /// <returns>Filtered collection.</returns>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> collection)
        {
            if (collection != null)
            {
                return collection.Where(c => !EqualityComparer<T>.Default.Equals(c, default(T)));
            }
            return collection;
        }

        #endregion

    }
}
