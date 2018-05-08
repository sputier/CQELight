using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore.Common
{
    internal enum ConfigurationType
    {
        SQLServer,
        SQLite
    }

    internal class DbContextConfiguration
    {
        #region Properties

        public ConfigurationType ConfigType { get; set; }
        public string ConnectionString { get; set; }

        #endregion

    }
}
