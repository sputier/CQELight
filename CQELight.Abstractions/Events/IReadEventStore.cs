using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions
{
    /// <summary>
    /// Contract interface for Event Store reading
    /// </summary>
    public interface IReadEventStore
    {

        IDomainEvent GetEventById();

    }
}
