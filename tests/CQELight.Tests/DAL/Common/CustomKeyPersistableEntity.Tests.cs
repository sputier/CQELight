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
    public class CustomKeyPersistableEntityTests : BaseUnitTestClass
    {
        #region Ctor & members

        private class TestCustomKeyClass : CustomKeyPersistableEntity
        {
            [PrimaryKey]
            public string MyDataKey { get; set; }
        }

        #endregion

        #region GetKeyValue

        [Fact]
        public void CustomKeyPersistableEntity_GetKeyValue_Should_Returns_ConcernedMember()
        {
            var c = new TestCustomKeyClass
            {
                MyDataKey = "MyTestPK"
            };
            c.GetKeyValue().Should().Be("MyTestPK");
        }

        #endregion

        #region IsKeySet

        [Fact]
        public void CustomKeyPersistableEntity_IsKeySet_Should_Returns_CorrectValue()
        {
            var c = new TestCustomKeyClass();
            c.IsKeySet().Should().BeFalse();

            var c2 = new TestCustomKeyClass
            {
                MyDataKey = "MyTestPK"
            };
            c2.IsKeySet().Should().BeTrue();
        }

        #endregion

    }
}
