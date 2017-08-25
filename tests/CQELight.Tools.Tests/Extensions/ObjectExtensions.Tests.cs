using CQELight.TestFramework;
using CQELight.Tools.Extensions;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tools.Tests.Extensions
{
    public class ObjectExtensionsTests : BaseUnitTestClass
    {

        #region SameTypeCheck

        [Fact]
        public void ObjectExtensions_SameTypeCheck_Nulls()
        {
            (null as object).SameTypeCheck(null as object).Should().BeTrue();
            (null as object).SameTypeCheck(null as Exception).Should().BeTrue();
        }

        [Fact]
        public void ObjectExtensions_SameTypeCheck_Tests()
        {
            var o = new object();
            var o2 = new object();
            var e = new Exception();

            o.SameTypeCheck(null).Should().BeFalse();
            (null as object).SameTypeCheck(o).Should().BeFalse();
            o.SameTypeCheck(e).Should().BeFalse();
            o.SameTypeCheck(o2).Should().BeTrue();
        }

        #endregion

        #region ToJson

        [Fact]
        public void ObjectExtensions_ToJson_Null()
        {
            (null as object).ToJson().Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public void ObjectExtensions_ToJson_AsExpected()
        {
            var d = new DateTime(2017, 1, 1);

            d.ToJson().Should().Be(JsonConvert.SerializeObject(d));
        }

        #endregion

    }
}
