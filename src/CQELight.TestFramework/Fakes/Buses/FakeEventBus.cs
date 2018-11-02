using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.TestFramework.Fakes.Buses
{
    /// <summary>
    /// Fake event bus used to test events.
    /// </summary>
    public class FakeEventBus : IDomainEventBus
    {
        #region Static members

        internal static FakeEventBus Instance;

        #endregion

        #region Members

        internal List<IDomainEvent> _events = new List<IDomainEvent>();

        #endregion

        #region Properties

        public IEnumerable<IDomainEvent> Events => _events.AsEnumerable();

        #endregion

        #region Ctor

        public FakeEventBus()
        {
            Instance = this;
        }

        #endregion

        #region IDomainEventBus methods

        /// <summary>
        /// Register asynchronously an event to be processed by the bus.
        /// </summary>
        /// <param name="event">Event to register.</param>
        /// <param name="context">Context associated to the event.</param>
        public Task PublishEventAsync(IDomainEvent @event, IEventContext context = null)
        {
            _events.Add(@event);
            return Task.CompletedTask;
        }

        public Task PublishEventRangeAsync(IEnumerable<(IDomainEvent @event, IEventContext context)> data)
        {
            _events.AddRange(data.Select(e => e.@event));
            return Task.CompletedTask;
        }

        #endregion

    }
}
