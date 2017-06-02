using System;
using System.Collections.Generic;
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

        #endregion

    }
}
