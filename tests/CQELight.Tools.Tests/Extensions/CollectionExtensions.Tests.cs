using CQELight.TestFramework;
using CQELight.Tools.Extensions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tools.Tests.Extensions
{
    public class CollectionExtensionsTests : BaseUnitTestClass
    {

        #region DoForEach

        [Fact]
        public void CollectionExtensions_DoForEach_ParamsTest()
        {
            Assert.Throws<ArgumentNullException>(() => (null as IEnumerable<object>).DoForEach(o => o.ToString()));
        }

        [Fact]
        public void CollectionExtensions_DoForEach_NullAction()
        {
            var list = new List<object>();

            list.DoForEach(null); // Should not throw
        }

        [Fact]
        public void CollectionExtensions_DoForEach_AsExpected()
        {
            var sb = new StringBuilder();
            var dates = new List<DateTime>
            {
                new DateTime(2010,1,1),
                new DateTime(2012,1,1),
                new DateTime(2014,1,1),
            };

            dates.DoForEach(d => sb.AppendLine(d.ToString("dd.MM.yyyy")));

            var result = sb.ToString();
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Contain("01.01.2010");
            result.Should().Contain("01.01.2012");
            result.Should().Contain("01.01.2014");
        }

        #endregion

    }
}
