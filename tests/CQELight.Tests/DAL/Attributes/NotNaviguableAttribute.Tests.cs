using CQELight.DAL.Attributes;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests.DAL.Attributes
{
    public class NotNaviguableAttributeTests : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region Ctor

        [Fact]
        public void NotNaviguableAttribute_Ctor_TestParams()
        {
            var na = new NotNaviguableAttribute();
            na.Mode.Should().Be(NavigationMode.All);

            var na2 = new NotNaviguableAttribute(NavigationMode.Create);
            na2.Mode.Should().Be(NavigationMode.Create);
        }

        #endregion

    }
}
