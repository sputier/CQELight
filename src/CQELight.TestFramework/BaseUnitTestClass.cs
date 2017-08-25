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
            //To define integration mode, we use xUnit collection. This allow use also to disable parallelism for integration.
            UnitTestTools.IsInIntegrationTestMode =
                GetType().GetCustomAttribute<CollectionAttribute>() != null;
        }

        #endregion

    }
}
