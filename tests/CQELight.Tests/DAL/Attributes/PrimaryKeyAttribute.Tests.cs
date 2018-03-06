using CQELight.DAL.Attributes;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests.DAL.Attributes
{
    public class PrimaryKeyAttributeTests : BaseUnitTestClass
    {

        #region ctor & members

        #endregion

        #region Ctor

        [Fact]
        public void PrimaryKeyAttribute_ctor_TestParams()
        {

            Assert.Throws<ArgumentNullException>(() => new PrimaryKeyAttribute(string.Empty));

        }

        [Fact]
        public void PrimaryKeyAttribute_ctor_AsExpected()
        {
            var pk = new PrimaryKeyAttribute("test");
            pk.KeyName.Should().Be("test");
        }

        #endregion

    }
}
