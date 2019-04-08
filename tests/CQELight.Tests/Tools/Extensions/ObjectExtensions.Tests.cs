using CQELight.TestFramework;
using CQELight.Tools.Extensions;
using CQELight.Tools.Serialisation;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
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

        private class JsonSerialized
        {
            public string PublicData { get; set; }
            private string _privateData;

            public JsonSerialized(string publicData, string privateData)
            {
                PublicData = publicData;
                _privateData = privateData;
            }
            public JsonSerialized()
            {

            }
        }

        [Fact]
        public void ObjectExtensions_ToJson_PrivateFields_Should_Be_Not_Be_Serialized_IfNotSpecified()
        {
            var obj = new JsonSerialized("public", "private");

            var result = obj.ToJson();

            result.Should().NotContain("_privateData");

        }

        [Fact]
        public void ObjectExtensions_ToJson_PrivateFields_Should_Be_Serialized()
        {
            var obj = new JsonSerialized("public", "private");

            var result = obj.ToJson(true);

            result.Should().Contain("_privateData");
        }

        [Fact]
        public void ObjectExtensions_ToJson_SpecificContracts_Should_Be_Used()
        {
            var contract = new Mock<IJsonContractDefinition>();

            contract.Setup(m => m.SetSerialisationPropertyContractDefinition(It.IsAny<JsonProperty>(), It.IsAny<MemberInfo>()))
                .Callback((JsonProperty prop, MemberInfo m) =>
                {
                    if(m.Name == "PublicData")
                    {
                        prop.ShouldSerialize = _ => false;
                    }
                });

            var obj = new JsonSerialized("public", "private");

            var result = obj.ToJson(contract.Object);

            result.Should().NotBeNullOrEmpty();
            result.Should().NotContain("PublicData");
        }

        #endregion

        #region In

        [Fact]
        public void ObjectExtensions_In_NotIn_Null()
        {
            "test".In(null).Should().BeFalse();
        }
        [Fact]
        public void ObjectExtensions_In_NotIn_Empty()
        {
            "test".In(new string[0]).Should().BeFalse();
        }

        [Fact]
        public void ObjectExtensions_In_In()
        {
            "test".In("foo", "bar", "test").Should().BeTrue();
        }

        #endregion

    }
}
