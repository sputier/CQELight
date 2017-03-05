using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Abstractions
{
    /// <summary>
    /// A public class that represents a Domain Event
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Unique id of the event.
        /// </summary>
        Guid Id { get; }
    }
}
