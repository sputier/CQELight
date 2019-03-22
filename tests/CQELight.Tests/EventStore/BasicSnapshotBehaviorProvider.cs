using CQELight.Abstractions.Events;
using CQELight.Abstractions.EventStore.Interfaces;
using CQELight.EventStore;
using CQELight.TestFramework;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests.EventStore
{
    public class BasicSnapshotBehaviorProviderTests : BaseUnitTestClass
    {
        #region Ctor & members

        private Mock<ISnapshotBehavior> _behaviorMock = new Mock<ISnapshotBehavior>();

        private class Event1 : BaseDomainEvent { }
        private class Event2 : BaseDomainEvent { }
        #endregion

        #region GetBehaviorForEventType

        [Fact]
        public void GetBehaviorForEventType_Should_Returns_GoodInstance_Or_Null()
        {
            var behavior = new BasicSnapshotBehaviorProvider(new Dictionary<Type, ISnapshotBehavior>
            {
                { typeof(Event1), _behaviorMock.Object }
            });

            behavior.GetBehaviorForEventType(typeof(Event1)).Should().BeSameAs(_behaviorMock.Object);
            behavior.GetBehaviorForEventType(typeof(Event2)).Should().BeNull();
        }

        #endregion
    }
}
