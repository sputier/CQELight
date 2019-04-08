using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Abstractions.EventStore;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.Extensions;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests.Extensions
{
    public class IEventSourcedAggregateExtensionsTests : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region GetSerializedState

        private class AggState : AggregateState { public string Data { get; set; } = "Data"; }

        private class MemberAgg : IEventSourcedAggregate
        {
            private AggState _state = new AggState();

            public void RehydrateState(IEnumerable<IDomainEvent> events)
            {
                throw new NotImplementedException();
            }
        }
        private class PropAgg : IEventSourcedAggregate
        {
            private AggState State { get; set; } = new AggState();

            public void RehydrateState(IEnumerable<IDomainEvent> events)
            {
                throw new NotImplementedException();
            }
        }

        private class NoStateAgg : IEventSourcedAggregate
        {
            public void RehydrateState(IEnumerable<IDomainEvent> events)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void GetSerializedState_Should_Returns_Json_FromMember_OrProperty()
        {
            Assert.Throws<InvalidOperationException>(() => new NoStateAgg().GetSerializedState());

            var memberState = new MemberAgg().GetSerializedState();
            memberState.Should().NotBeNullOrWhiteSpace();

            var propState = new PropAgg().GetSerializedState();
            propState.Should().NotBeNullOrWhiteSpace();
        }

        #endregion

    }
}
