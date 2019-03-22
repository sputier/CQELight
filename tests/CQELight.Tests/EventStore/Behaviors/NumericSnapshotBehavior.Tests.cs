using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.EventStore.Snapshots;
using CQELight.TestFramework;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests.EventStore.Behaviors
{
    public class NumericSnapshotBehaviorTest : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region IsSnapshotNeeded

        [Fact]
        public void IsSnapshotNeedShould_ReturnsTrue_Every_X_Sequence()
        {
            var b = new NumericSnapshotBehavior(10);
            var firstEventMock = new Mock<IDomainEvent>();
            firstEventMock.Setup(m => m.Sequence).Returns(1);

            b.IsSnapshotNeeded(firstEventMock.Object).Should().BeFalse();

            var secondEventMock = new Mock<IDomainEvent>();
            secondEventMock.Setup(m => m.Sequence).Returns(2);

            b.IsSnapshotNeeded(secondEventMock.Object).Should().BeFalse();

            var laterEventMock = new Mock<IDomainEvent>();
            laterEventMock.Setup(m => m.Sequence).Returns(11);

            b.IsSnapshotNeeded(laterEventMock.Object).Should().BeTrue();

            var secondSnapshotEvent = new Mock<IDomainEvent>();
            secondSnapshotEvent.Setup(m => m.Sequence).Returns(21);

            b.IsSnapshotNeeded(secondSnapshotEvent.Object).Should().BeTrue();
        }

        #endregion

        #region GenerateSnapshot

        private class SnapshotState : AggregateState
        {
            public SnapshotState()
            {
                AddHandler<BehaviorEventTest>(On);
            }

            private void On(BehaviorEventTest obj)
            {
            }
        }
        private class BehaviorEventTest : BaseDomainEvent { }

        [Fact]
        public void GenerateSnapshot_Should_Grab_FromState()
        {
            var state = new SnapshotState();
            for (int i = 0; i < 10; i++)
            {
                state.Apply(new BehaviorEventTest());
            }

            var b = new NumericSnapshotBehavior(10);
            var events = b.GenerateSnapshot(state);
            events.Should().HaveCount(10);
        }

        #endregion
    }
}
