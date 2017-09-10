using System;
using System.Collections;
using System.Text;

namespace CQELight.Tools.Extensions
{
    /// <summary>
    /// Extensions class for non generic collection.
    /// </summary>
    public static class NonGenericLINQExtensions
    {

        #region Public static methods
        
        /// <summary>
        /// Check if the collection contains at least one element.
        /// </summary>
        /// <param name="enumerable">Collection to check.</param>
        /// <returns>True if one or more elements are in, false otherwise</returns>
        public static bool Any(this IEnumerable enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }
            return enumerable.GetEnumerator().MoveNext();
        }

        #endregion

    }
}
