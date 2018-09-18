using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore.Common
{
    public enum ConfigurationType
    {
        SQLServer,
        SQLite
    }

    public class DbContextConfiguration
    {
        #region Properties

        internal ConfigurationType ConfigType { get; set; }
        internal string ConnectionString { get; set; }

        #endregion

        #region Ctor

        internal DbContextConfiguration() { }

        #endregion

    }
}
