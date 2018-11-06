using CQELight.Abstractions.Dispatcher.Interfaces;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Dispatcher;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQELight.Abstractions.DDD
{
    /// <summary>
    /// Base definition for aggregate. An aggregate is an entity that manage a bunch of entities and value-objects by keeping they consistent.
    /// </summary>
    /// <typeparam name="T">Type of aggregate Id</typeparam>
    public abstract class AggregateRoot<T> : Entity<T>
    {
        #region Members

        private readonly List<(IDomainEvent Event, IEventContext Context)> _domainEvents = new List<(IDomainEvent, IEventContext)>();
        private readonly SemaphoreSlim _lockSecurity = new SemaphoreSlim(1);

        #endregion

        #region Properties

        /// <summary>
        /// The unique ID of the aggregate to be used in external systems.
        /// </summary>
        public Guid AggregateUniqueId { get; protected set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Parameterless ctor.
        /// </summary>
        protected AggregateRoot()
        {
            AggregateUniqueId = Guid.NewGuid();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// List of domain events associated to the aggregate.
        /// Do not use this collection to dispatch them but use the public method "DispatchDomainEvents"
        /// </summary>
        public virtual IEnumerable<IDomainEvent> DomainEvents => _domainEvents.Select(m => m.Event).AsEnumerable();

        /// <summary>
        /// Dispatch all domain events holded by the aggregate.
        /// </summary>
        public Task DispatchDomainEventsAsync()
            => DispatchDomainEventsAsync(null);

        /// <summary>
        /// Dispatch all domain events holded by the aggregate with a specified dispatcher.
        /// </summary>
        /// <param name="dispatcher">Dispatcher used for publishing.</param>
        public async Task DispatchDomainEventsAsync(IDispatcher dispatcher)
        {
            await _lockSecurity.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_domainEvents.Count > 0)
                {
                    foreach (var evt in _domainEvents.Select(e => e.Event))
                    {
                        var props = evt.GetType().GetAllProperties();
                        var aggIdProp = props.FirstOrDefault(p => p.Name == nameof(IDomainEvent.AggregateId));
                        aggIdProp?.SetMethod?.Invoke(evt, new object[] { AggregateUniqueId });
                        var aggTypeProp = props.FirstOrDefault(p => p.Name == nameof(IDomainEvent.AggregateType));
                        aggTypeProp?.SetMethod?.Invoke(evt, new object[] { GetType() });
                    }
                    if (dispatcher == null)
                    {
                        await CoreDispatcher.PublishEventRangeAsync(_domainEvents).ConfigureAwait(false);
                    }
                    else
                    {
                        await dispatcher.PublishEventRangeAsync(_domainEvents).ConfigureAwait(false);
                    }
                }
                _domainEvents.Clear();
            }
            finally
            {
                _lockSecurity.Release();
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Add a domain event to the aggregate events collection.
        /// </summary>
        /// <param name="newEvent">Event to add.</param>
        /// <param name="ctx">Related context.</param>
        protected virtual void AddDomainEvent(IDomainEvent newEvent, IEventContext ctx = null)
        {
            _lockSecurity.Wait();
            try
            {
                if (newEvent != null)
                {
                    _domainEvents.Add((newEvent, ctx));
                }
            }
            finally
            {
                _lockSecurity.Release();
            }
        }

        #endregion

    }
}
