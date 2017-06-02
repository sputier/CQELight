﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.Abstractions.Events.Interfaces
{
    /// <summary>
    /// A contract interface for domain event handlers.
    /// </summary>
    /// <typeparam name="T">Domain Event.</typeparam>
    public interface IDomainEventHandler<T> where T : IDomainEvent
    {
        /// <summary>
        /// Handle the domain event.
        /// </summary>
        /// <param name="domainEvent">Domain event to handle.</param>
        /// <param name="context">Associated context.</param>
        void Handle(T domainEvent, IEventContext context = null);
    }
}