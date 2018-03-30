using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore.Common
{

    enum ConfigurationType
    {
        SQLServer,
        SQLite
    }

    class DbContextConfiguration
    {

        #region Properties

        public ConfigurationType ConfigType { get; set; }
        public string ConnectionString { get; set; }

        #endregion

    }
}
