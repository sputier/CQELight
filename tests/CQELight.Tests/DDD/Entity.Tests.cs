using CQELight.Abstractions.DDD;
using FluentAssertions;
using System;
using Xunit;

namespace CQELight.Abstractions.Tests.DDD
{

    public class EntityTests
    {

        #region Nested classes

        private class TestEntity : Entity
        {

            public void SetIdForTest(Guid id)
            {
                Id = id;
            }
        }

        #endregion

        private TestEntity Ent => new TestEntity();

        #region Equals

        [Fact]
        public void Entity_Equals_NullObject()
        {
            Ent.Equals(null).Should().BeFalse();
            (Ent == null).Should().BeFalse();
            (null == Ent).Should().BeFalse();
            (null != Ent).Should().BeTrue();
            (Ent != null).Should().BeTrue();


            Entity o = null;
            Entity o2 = null;
            (o == o2).Should().BeTrue();
            (o != o2).Should().BeFalse();
        }

        [Fact]
        public void Entity_Equals_DifferentType_NotEntity()
        {
            Ent.Equals(16).Should().BeFalse();
        }

        [Fact]
        public void Entity_Equals_DifferentEntityType()
        {
            Ent.Equals(new TestEntity()).Should().BeFalse();
        }

        [Fact]
        public void Entity_Equals_DifferentID()
        {
            var ent1 = new TestEntity();
            var ent2 = new TestEntity();
            ent1.SetIdForTest(Guid.NewGuid());
            ent2.SetIdForTest(Guid.NewGuid());
            ent1.Equals(ent2).Should().BeFalse();
        }

        [Fact]
        public void Entity_Equals_SameID()
        {
            var ent1 = new TestEntity();
            var ent2 = new TestEntity();
            var id = Guid.NewGuid();
            ent1.SetIdForTest(id);
            ent2.SetIdForTest(id);
            ent1.Equals(ent2).Should().BeTrue();
        }


        #endregion

    }
}
