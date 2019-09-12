using CQELight.DAL.MongoDb.Mapping;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CQELight.DAL.MongoDb.Integration.Tests
{
    public class MappingInfoTests : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region MappingInfo

        [Fact]
        public void MappingInfos_SimpleClass()
        {
            var m = new MappingInfo(typeof(User));
            m.Should().NotBeNull();

            m.CollectionName.Should().Be("User");
            m.DatabaseName.Should().Be("DefaultDatabase");
            m.EntityType.Should().BeSameAs(typeof(User));
            m.IdProperty.Should().Be("Id");
        }

        [Fact]
        public void MappingInfos_SimpleIndex()
        {
            var m = new MappingInfo(typeof(Tag));
            m.Should().NotBeNull();

            m.CollectionName.Should().Be("Tag");
            m.DatabaseName.Should().Be("DefaultDatabase");
            m.EntityType.Should().BeSameAs(typeof(Tag));
            m.IdProperty.Should().Be("Id");
            m.Indexes.Should().HaveCount(1);
            m.Indexes.First().Properties.First().Should().Be("Value");
            m.Indexes.First().Unique.Should().BeTrue();
        }

        [Fact]
        public void MappingInfos_ComplexIndex()
        {
            var m = new MappingInfo(typeof(Comment));
            m.Should().NotBeNull();

            m.CollectionName.Should().Be("Comment");
            m.DatabaseName.Should().Be("DefaultDatabase");
            m.EntityType.Should().BeSameAs(typeof(Comment));
            m.IdProperty.Should().Be("Id");
            m.Indexes.Should().HaveCount(1);
            m.Indexes.First().Properties.Should().HaveCount(3);
            m.Indexes.First().Properties.Any(p => p  == "Value").Should().BeTrue();
            m.Indexes.First().Properties.Any(p => p  == "Owner").Should().BeTrue();
            m.Indexes.First().Properties.Any(p => p  == "Post").Should().BeTrue();
            m.Indexes.First().Unique.Should().BeFalse();
        }

        #endregion

    }
}
