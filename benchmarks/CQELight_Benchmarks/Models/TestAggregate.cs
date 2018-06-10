using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight_Benchmarks.Models
{
    class TestAggregate : AggregateRoot<Guid>
    {

        #region Members

        private TestAggregateState _state;

        #endregion

        #region Private class

        private class TestAggregateState : AggregateState
        {

            #region Properties

            public int AggInt { get; set; }
            public string AggString { get; set; }

            #endregion

            #region Ctor

            public TestAggregateState()
            {
                AddHandler<AggregateEvent>(AggregateEventHandler);
            }

            #endregion

            #region Public methods

            private void AggregateEventHandler(AggregateEvent evt)
            {
                AggInt = evt.AggregateIntValue;
                AggString = evt.AggregateStringValue;
            }


            #endregion

        }

        #endregion

        #region Ctor

        public TestAggregate(IEnumerable<IDomainEvent> events)
        {
            _state = new TestAggregateState();
            _state.ApplyRange(events);
        }

        #endregion

    }
}
