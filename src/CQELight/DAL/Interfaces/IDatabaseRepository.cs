using CQELight.DAL.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.Interfaces
{
    /// <summary>
    /// Contract interface for repository upon database.
    /// </summary>
    /// <typeparam name="T">Type of database entity.</typeparam>
    public interface IDatabaseRepository<T> : IDataReaderRepository<T>, IDataUpdateRepository<T>, IDisposable
        where T : IPersistableEntity
    {
    }
}
