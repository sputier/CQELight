using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CQELight.Abstractions.Tests.DDD
{
    #region Nested classes

    public class TestDomainEvent : BaseDomainEvent
    {
        public string Data { get; set; }
        public new Guid? AggregateId { get; set; }
    }


    public class AggregateIdTest : AggregateRoot<Guid>
    {
        public void SetIDForTest(Guid id)
        {
            this.Id = id;
        }

        public void SetNewId()
        {
            Id = Guid.NewGuid();
        }
        public void SimulateAction()
        {
            var domainEvent = new TestDomainEvent() { Data = "test data", AggregateId = Id };
            this.AddDomainEvent(domainEvent);
        }
    }

    #endregion

    public class AggregateTests : BaseUnitTestClass
    {
        #region Ctor

        [Fact]
        public void AggregateRoot_ctor_AsExpected()
        {
            AggregateIdTest o = new AggregateIdTest();

            o.DomainEvents.Count().Should().Be(0);
            o.SimulateAction();
            o.DomainEvents.Count().Should().Be(1);
            (o.DomainEvents.First() as TestDomainEvent).Data.Should().Be("test data");
        }

        #endregion

        #region SetNewId

        [Fact]
        public void Aggregate_SetNewId_NotDefined()
        {
            var agg = new AggregateIdTest();
            agg.SetNewId();
            agg.Id.Should().NotBeEmpty();
        }

        #endregion

    }
}
