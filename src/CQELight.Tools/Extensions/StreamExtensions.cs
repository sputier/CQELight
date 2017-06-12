using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

namespace CQELight.Tools.Extensions
{
    /// <summary>
    /// Extension class for Stream objects.
    /// </summary>
    public static class StreamExtensions
    {

        #region Public static methods

        /// <summary>
        /// Get a string from a PipeStream.
        /// </summary>
        /// <param name="stream">PipeStream to extract string.</param>
        /// <returns>Found string in UTF8.</returns>
        public static string ReadString(this PipeStream stream)
        {
            if (stream == null)
            {
                return string.Empty;
            }
            var dataBytes = new List<byte>();
            var buffer = new byte[1024];
            do
            {
                stream.Read(buffer, 0, buffer.Length);
                dataBytes.AddRange(buffer);
                buffer = new byte[buffer.Length];
            }
            while (!stream.IsMessageComplete);
            return Encoding.UTF8.GetString(dataBytes.ToArray()).Trim('\0');
        }

        #endregion

    }
}
