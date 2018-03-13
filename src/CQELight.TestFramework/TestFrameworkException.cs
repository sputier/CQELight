using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.TestFramework
{
    /// <summary>
    /// Base exception class for the testing framework.
    /// </summary>
    public class TestFrameworkException : Exception
    {

        #region Ctor

        /// <summary>Initializes a new instance of the <see cref="Exception"></see> class with a specified error message.</summary>
        /// <param name="message">The message that describes the error.</param>
        public TestFrameworkException(string message) : base(message)
        {
        }

        #endregion
    }
}
