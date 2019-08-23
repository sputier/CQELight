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
        public void TableAttribute_ctor_AsExpected()
        {
            var table = new TableAttribute("test");
            table.TableName.Should().Be("test");
            table.SchemaName.Should().BeNullOrWhiteSpace();

            var table2 = new TableAttribute("test", "schema");
            table2.TableName.Should().Be("test");
            table2.SchemaName.Should().Be("schema");
        }

        #endregion

    }
}
