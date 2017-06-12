using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CQELight.Tools.Extensions
{
    /// <summary>
    /// Extension class for string class.
    /// </summary>
    public static class StringExtensions
    {

        #region Public static methods

        /// <summary>
        /// Deserialize object from json.
        /// </summary>
        /// <param name="json">Json to deserialize.</param>
        /// <paramtype name="T">Expected object type.</paramtype>
        /// <returns>Instance de type voulu</returns>
        public static T FromJson<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }

            var ret = FromJson(json, typeof(T));
            if (ret is T)
            {
                return (T)ret;
            }
            return default(T);
        }

        /// <summary>
        /// Deserialize object from json.
        /// </summary>
        /// <param name="json">Json to deserialize.</param>
        /// <param name="objectType">Expected object type.</param>
        /// <returns>Instance de type voulu</returns>
        public static object FromJson(this string json, Type objectType)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            return JsonConvert.DeserializeObject(json, objectType);
        }

        /// <summary>
        /// Write the string to a stream.
        /// </summary>
        /// <param name="value">Value to write.</param>
        /// <param name="str">Stream to write in.</param>
        /// <returns>Number of written chars..</returns>
        public static int WriteToStream(this string value, Stream str)
        {
            if (str == null || !str.CanWrite)
            {
                throw new ArgumentNullException(nameof(str), "StringExtensions.WriteToStream() : Stream cannot be null and must be writable.");
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }
            byte[] outBuffer = Encoding.UTF8.GetBytes(value);
            str.Write(outBuffer, 0, outBuffer.Length);
            str.Flush();

            return outBuffer.Length;
        }

        #endregion

    }
}
