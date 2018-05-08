using CQELight.Abstractions.DDD;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Abstractions.Tests.DDD
{
    public class ValueObjectTests
    {
        #region Nested classes

        public class IntValueObject : ValueObject<IntValueObject>
        {
            public int Prop { get; set; }

            protected override bool EqualsCore(IntValueObject other)
            {
                return Prop.Equals(other.Prop);
            }

            protected override int GetHashCodeCore()
            {
                return Prop.GetHashCode();
            }
        }

        public class StringValueObject : ValueObject<StringValueObject>
        {
            public string Prop { get; set; }

            protected override bool EqualsCore(StringValueObject other)
            {
                return Prop.Equals(other.Prop);
            }

            protected override int GetHashCodeCore()
            {
                return Prop.GetHashCode();
            }
        }

        #endregion

        #region Equals

        [Fact]
        public void ValueObject_Equals_NotEquals()
        {
            var i1 = new IntValueObject { Prop = 1 };
            var i2 = new IntValueObject { Prop = 2 };

            i1.Equals(i2).Should().BeFalse();
        }

        [Fact]
        public void ValueObject_Equals_NotSameType()
        {
            var i1 = new IntValueObject { Prop = 1 };
            var s2 = new StringValueObject { Prop = "1" };

            object.ReferenceEquals(i1, s2).Should().BeFalse();
            i1.Equals(s2).Should().BeFalse();
        }

        [Fact]
        public void ValueObject_Equals_Equals()
        {
            var i1 = new IntValueObject { Prop = 1 };
            var i2 = new IntValueObject { Prop = 1 };

            object.ReferenceEquals(i1, i2).Should().BeFalse();
            i1.Equals(i2).Should().BeTrue();
        }
        #endregion

        #region GetHashCode

        [Fact]
        public void ValueObject_GetHashCode()
        {
            var i1 = new IntValueObject { Prop = 123456798 };

            i1.GetHashCode().Should().Be(123456798.GetHashCode());
        }

        #endregion

        #region EqualityOp

        [Fact]
        public void ValueObject_EqualityOp_NotEquals()
        {
            var i1 = new IntValueObject { Prop = 1 };
            var i2 = new IntValueObject { Prop = 2 };

            (i1 == i2).Should().BeFalse();
            (i1 == null).Should().BeFalse();
            ((IntValueObject)null == (IntValueObject)null).Should().BeTrue();
            (null == i1).Should().BeFalse();
        }

        [Fact]
        public void ValueObject_EqualityOp_Equals()
        {
            var i1 = new IntValueObject { Prop = 1 };
            var i2 = new IntValueObject { Prop = 1 };

            ReferenceEquals(i1, i2).Should().BeFalse();
            (i1 == i2).Should().BeTrue();
        }

        #endregion

        #region InequalityOp

        [Fact]
        public void ValueObject_InequalityOp_NotEquals()
        {
            var i1 = new IntValueObject { Prop = 1 };
            var i2 = new IntValueObject { Prop = 2 };

            (i1 != i2).Should().BeTrue();
            (i1 != null).Should().BeTrue();
            (null != i1).Should().BeTrue();
        }

        [Fact]
        public void ValueObject_InequalityOp_Equals()
        {
            var i1 = new IntValueObject { Prop = 1 };
            var i2 = new IntValueObject { Prop = 1 };

            ReferenceEquals(i1, i2).Should().BeFalse();
            (i1 != i2).Should().BeFalse();
        }

        #endregion

    }
}
