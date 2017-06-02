using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Tools.Extensions
{
    /// <summary>
    /// Class of extensions methods for objects.
    /// </summary>
    public static class ObjectExtensions
    {

        #region Public static methods

        /// <summary>
        /// Check if type are same.
        /// </summary>
        /// <param name="obj">Other objet instance to compare with.</param>
        /// <param name="value">Object instance.</param>
        /// <returns>If both object are same type.</returns>
        public static bool SameTypeCheck(this object value, object obj)
            => value?.GetType() == obj?.GetType();


        #endregion

    }
}
