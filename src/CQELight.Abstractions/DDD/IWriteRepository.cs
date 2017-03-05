using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions
{
    /// <summary>
    /// Contract interface for Repository implementation.
    /// </summary>
    public interface IWriteRepository<T> where T : IAggregateRoot
    {
    }
}
