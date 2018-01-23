using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Tools.Extensions
{
    /// <summary>
    /// Some extensions methods for ILogger.
    /// </summary>
    public static class ILoggerExtensions
    {
        
        #region Public static methods

        /// <summary>
        /// Log an error on multiples lines.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="errorLines">Collection of lines to log as error.</param>
        public static void LogErrorMultilines(this ILogger logger, params string[] errorLines)
            => logger.LogError(string.Join(Environment.NewLine, errorLines));

        /// <summary>
        /// Log a warning on multiples lines.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="errorLines">Collection of lines to log as warning.</param>
        public static void LogWarningMultilines(this ILogger logger, params string[] warningLines)
            => logger.LogWarning(string.Join(Environment.NewLine, warningLines));

        #endregion

    }
}
