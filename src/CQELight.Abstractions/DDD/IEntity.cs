using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions
{
    /// <summary>
    /// Contract interface for Entity
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Id of the entity
        /// </summary>
        Guid Id { get; }

    }
}
