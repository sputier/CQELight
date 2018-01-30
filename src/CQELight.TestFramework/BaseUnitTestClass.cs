using System;
using System.Reflection;
using Xunit;

namespace CQELight.TestFramework
{
    /// <summary>
    /// Base class for unit testing classes.
    /// </summary>
    public class BaseUnitTestClass
    {

        #region Ctor

        /// <summary>
        /// Base constructeur for class.
        /// </summary>
        protected BaseUnitTestClass()
        {
            UnitTestTools.IsInUnitTestMode = true;
            UnitTestTools.IsInIntegrationTestMode = GetType().Assembly.GetName().Name.Contains(".Integration.");
        }

        #endregion

    }
}
