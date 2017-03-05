using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions
{
    /// <summary>
    /// Contract interface for reading repository.
    /// </summary>
    /// <typeparam name="T">Aggregate type.</typeparam>
    public interface IReadRepository<T> where T : IAggregateRoot
    {

        IAggregateRoot GetById(Guid id);


    }
}
