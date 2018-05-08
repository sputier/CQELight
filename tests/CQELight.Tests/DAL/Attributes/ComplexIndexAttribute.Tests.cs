using CQELight.DAL.Attributes;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests.DAL.Attributes
{
    public class ComplexIndexAttributeTests : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region Ctor

        [Fact]
        public void ComplexIndexAttribute_Ctor_TestParams()
        {
            var ci = new ComplexIndexAttribute("test", "test2");
            ci.IsUnique.Should().BeFalse();
            ci.PropertyNames.Should().Contain("test");
            ci.PropertyNames.Should().Contain("test2");
            ci.IndexName.Should().BeNullOrWhiteSpace();

            var ci2 = new ComplexIndexAttribute(new[] { "test", "test2" }, true);
            ci2.IsUnique.Should().BeTrue();
            ci2.PropertyNames.Should().Contain("test");
            ci2.PropertyNames.Should().Contain("test2");
            ci2.IndexName.Should().BeNullOrWhiteSpace();

            var ci3 = new ComplexIndexAttribute(new[] { "test", "test2" }, true);
            ci3.IsUnique.Should().BeTrue();
            ci3.PropertyNames.Should().Contain("test");
            ci3.PropertyNames.Should().Contain("test2");
            ci3.IndexName.Should().BeNullOrWhiteSpace();
        }

        #endregion

    }
}
