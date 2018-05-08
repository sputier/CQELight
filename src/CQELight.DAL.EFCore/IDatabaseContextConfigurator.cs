using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.EFCore
{
    /// <summary>
    /// Contract interface to configure the context for database connection.
    /// </summary>
    public interface IDatabaseContextConfigurator
    {
        /// <summary>
        /// Configure ContextOptionsBuilder to set the connection string.
        /// </summary>
        /// <param name="optionsBuilder">Context options builder.</param>
        void ConfigureConnectionString(DbContextOptionsBuilder optionsBuilder);
    }
}
