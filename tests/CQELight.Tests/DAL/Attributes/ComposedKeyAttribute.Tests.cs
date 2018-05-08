using CQELight.DAL.Attributes;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests.DAL.Attributes
{
    public class ComposedKeyAttributeTests : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region Ctor

        [Fact]
        public void ComposedKeyAttribute_Ctor_TestParams()
        {
            Assert.Throws<ArgumentNullException>(() => new ComposedKeyAttribute(null));
            Assert.Throws<InvalidOperationException>(() => new ComposedKeyAttribute("test"));
        }

        [Fact]
        public void ComposedKeyAttribute_ctor_AsExpected()
        {
            var c = new ComposedKeyAttribute("test", "test2");
            c.PropertyNames.Should().Contain("test");
            c.PropertyNames.Should().Contain("test2");
        }

        #endregion

    }
}
