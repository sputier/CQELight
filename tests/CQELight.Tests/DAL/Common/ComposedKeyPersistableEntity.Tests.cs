using CQELight.DAL.Attributes;
using CQELight.DAL.Common;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests.DAL.Common
{
    public class ComposedKeyPersistableEntityTests : BaseUnitTestClass
    {
        #region Ctor & members

        [ComposedKey(nameof(FirstMember), nameof(SecondMember), nameof(ThirdMember))]
        private class TestComposedKeyClass : ComposedKeyPersistableEntity
        {
            public string FirstMember { get; set; }
            public string SecondMember { get; set; }
            public int ThirdMember { get; set; }
        }

        #endregion

        #region GetKeyValue

        [Fact]
        public void GetKeyValue_Should_Returns_ComposedKeyAsString()
        {
            var c = new TestComposedKeyClass
            {
                FirstMember = "First",
                SecondMember = "Second",
                ThirdMember = 3
            };
            c.GetKeyValue().ToString().Should().Be("First,Second,3");
        }

        #endregion

        #region IsKeySet

        [Fact]
        public void IsKeySet_Should_Returns_CorrectValue()
        {
            var c = new TestComposedKeyClass();
            c.IsKeySet().Should().BeFalse();

            var c2 = new TestComposedKeyClass
            {
                FirstMember = "First",
                SecondMember = "Second",
                ThirdMember = 3
            };
            c2.IsKeySet().Should().BeTrue();
        }

        #endregion

    }
}
