using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.TestFramework
{
    /// <summary>
    /// Bunch of tools only use in Unit test
    /// </summary>
    public static class UnitTestTools
    {
        /// <summary>
        /// Flag to indicates if we're running in a unit test context.
        /// </summary>
        public static bool IsInUnitTestMode { get; internal set; } = false;
        /// <summary>
        /// Flag to indicates if we're running in integretation test context.
        /// </summary>
        public static bool IsInIntegrationTestMode { get; internal set; } = false;
    }
}
