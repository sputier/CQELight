using CQELight.DAL.Attributes;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests.DAL.Attributes
{
    public class KeyStorageOfAttributeTests : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region ctor

        [Fact]
        public void KeyStorageOfAttribute_Ctor_AsExpected()
        {
            var ko = new KeyStorageOfAttribute("test");
            ko.PropertyName.Should().Be("test");
        }

        #endregion

    }
}
