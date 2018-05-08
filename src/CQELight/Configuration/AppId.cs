using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Configuration
{
    /// <summary>
    /// Unique identifier of application.
    /// </summary>
    public struct AppId
    {
        #region Properties

        /// <summary>
        /// Current value of AppId
        /// </summary>
        public Guid Value { get; private set; }
        /// <summary>
        /// Human friendly alias for an AppId.
        /// </summary>
        public string Alias { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Creation of an AppId with a new value.
        /// </summary>
        /// <param name="value">Value of the Id.</param>
        /// <param name="alias">Alias of the appId</param>
        public AppId(Guid value, string alias = "")
        {
            if(value == Guid.Empty)
            {
                throw new ArgumentException("AppId.Ctor() : Value must be provided.", nameof(value));
            }
            Value = value;
            Alias = alias;
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Generate a new AppId with a specified alias.
        /// </summary>
        /// <param name="alias">Alias of the appId. If not specified, it will not be used.</param>
        /// <returns></returns>
        public static AppId Generate(string alias = "")
            => new AppId(Guid.NewGuid(), alias);

        #endregion

    }
}
