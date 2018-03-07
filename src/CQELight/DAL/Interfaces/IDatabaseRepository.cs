using CQELight.DAL.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.Interfaces
{
    /// <summary>
    /// Contract interface for repository on databases.
    /// </summary>
    /// <typeparam name="T">Type of entity to manage into database.</typeparam>
    public interface IDatabaseRepository<T> : IDataReaderRepository<T>, IDataUpdateRepository<T>, ISqlRepository, IDisposable
        where T : BaseDbEntity
    {

    }
}
