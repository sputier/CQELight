using CQELight.Tools.Serialisation;
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
        /// Deserialize base object from json.
        /// You need to be sure of type you receive to 
        /// unbox it. If you alread have the type, use
        /// another "FromJson" method.
        /// </summary>
        /// <param name="json">Json to deserialize.</param>
        /// <returns>Object instance</returns>
        public static object FromJson(this string json, bool deserializePrivateFields = false)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            return JsonConvert.DeserializeObject(json,
                deserializePrivateFields
                ?
                    new JsonSerializerSettings
                    {
                        ContractResolver = new JsonDeserialisationContractResolver(new AllFieldSerialisationContract())
                    }
                : null);
        }

        /// <summary>
        /// Deserialize object from json.
        /// </summary>
        /// <param name="json">Json to deserialize.</param>
        /// <param name="deserializePrivateFields">Indicates if private fields should be deserialized.</param>
        /// <paramtype name="T">Expected object type.</paramtype>
        /// <returns>Object instance</returns>
        public static T FromJson<T>(this string json, bool deserializePrivateFields = false)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }

            var ret = FromJson(json, typeof(T), deserializePrivateFields);
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
        /// <param name="settings">Custom json settings</param>
        /// <paramtype name="T">Expected object type.</paramtype>
        /// <returns>Object instance</returns>
        public static T FromJson<T>(this string json, JsonSerializerSettings settings)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }

            var ret = FromJson(json, typeof(T), settings);
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
        /// <param name="deserializePrivateFields">Indicates if private fields should be deserialized.</param>
        /// <param name="objectType">Expected object type.</param>
        /// <returns>Object instance.</returns>
        public static object FromJson(this string json, Type objectType, bool deserializePrivateFields = false)
            => FromJson(json, objectType, deserializePrivateFields
                ? new JsonSerializerSettings
                {
                    ContractResolver = new JsonDeserialisationContractResolver(new AllFieldSerialisationContract())
                }
                : null);

        /// <summary>
        /// Deserialize object from json.
        /// </summary>
        /// <param name="json">Json to deserialize.</param>
        /// <param name="objectType">Expected object type.</param>
        /// <param name="settings">Custom json settings</param>
        /// <returns>Object instance</returns>
        public static object FromJson(this string json, Type objectType, JsonSerializerSettings settings)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            return JsonConvert.DeserializeObject(json, objectType, settings ?? JsonDeserialisationContractResolver.DefaultDeserializeSettings);
        }

        /// <summary>
        /// Write the string to a stream.
        /// </summary>
        /// <param name="value">Value to write.</param>
        /// <param name="str">Stream to write in.</param>
        /// <returns>Number of written chars.</returns>
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
