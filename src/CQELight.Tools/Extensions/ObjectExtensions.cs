using Newtonsoft.Json;
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

        /// <summary>
        /// Retrieves Json data from an object.
        /// </summary>
        /// <param name="value">Objet which we want Json.</param>
        /// <returns>Json string if object is not null.</returns>
        public static string ToJson(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(obj, Formatting.None);
        }

        #endregion

    }
}
