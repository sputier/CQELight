using CQELight.DAL.Attributes;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests.DAL.Attributes
{
    public class ForeignKeyAttributeTests : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region ctor

        [Fact]
        public void ForeignKeyAttribute_Ctor_AsExpected()
        {
            var fk = new ForeignKeyAttribute("property", true);
            fk.DeleteCascade.Should().BeTrue();
            fk.InversePropertyName.Should().Be("property");

            var fk2 = new ForeignKeyAttribute();
            fk2.DeleteCascade.Should().BeFalse();
            fk2.InversePropertyName.Should().BeNullOrWhiteSpace();
        }

        #endregion

    }
}
