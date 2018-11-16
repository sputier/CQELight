using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.EFCore
{
    internal static class EFCoreInternalExecutionContext
    {

        #region Properties

        public static bool DisableLogicalDeletion { get; set; }

        #endregion

        #region Public static methods

        public static void ParseEFCoreOptions(EFCoreOptions options)
        {
            DisableLogicalDeletion = options.DisableLogicalDeletion;
        }

        #endregion

    }
}
