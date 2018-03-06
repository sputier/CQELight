using CQELight.DAL.Attributes;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Tests.DAL.Attributes
{
    public class TableAttributeTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region Ctor

        [Fact]
        public void TableAttribute_Ctor_TestParams()
        {
            Assert.Throws<ArgumentException>(() => new TableAttribute(string.Empty));
        }

        [Fact]
        public void TableAttribute_ctor_AsExpected()
        {
            var table = new TableAttribute("test");
            table.TableName.Should().Be("test");
            table.SchemaName.Should().Be("dbo");

            var table2 = new TableAttribute("test", "schema");
            table2.TableName.Should().Be("test");
            table2.SchemaName.Should().Be("schema");
        }

        #endregion

    }
}
