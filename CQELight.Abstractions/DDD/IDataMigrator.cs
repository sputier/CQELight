using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions.DDD
{
    /// <summary>
    /// Contract interface for Database migration.
    /// </summary>
    public interface IDataMigrator
    {
        /// <summary>
        /// Execute migration asynchronously.
        /// </summary>
        Task MigrateAsync();
    }
}
