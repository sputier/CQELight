using CQELight.Abstractions.DDD;
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

        public Task<Result> PublishEventAsync(IDomainEvent @event, IEventContext context = null)
        {
            _events.Add(@event);
            return Task.FromResult(Result.Ok());
        }

        public Task<Result> PublishEventRangeAsync(IEnumerable<IDomainEvent> events)
        {
            _events.AddRange(events);
            return Task.FromResult(Result.Ok());
        }

        #endregion

    }
}
