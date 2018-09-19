using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight_Benchmarks.Models
{
    partial class TestAggregate : EventSourcedAggregate<Guid, TestAggregateState>
    {

        #region Ctor

        public TestAggregate()
        {
            State = new TestAggregateState();
        }

        #endregion

    }
}
