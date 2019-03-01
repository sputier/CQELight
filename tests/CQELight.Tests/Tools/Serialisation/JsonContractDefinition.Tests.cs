using CQELight.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using CQELight.Tools.Extensions;
using Xunit;
using FluentAssertions;
using CQELight.Tools.Serialisation;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using Newtonsoft.Json;

namespace CQELight.Tools.Tests.Serialisation
{
    public class JsonContractDefinitionTests : BaseUnitTestClass
    {

        #region Ctor & members

        public class SerialisableClass
        {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
            public string Prop3 { get; set; }
        }

        public class AntiProp2Contract : IJsonContractDefinition
        {
            public void SetDeserialisationPropertyContractDefinition(JsonProperty property, MemberInfo memberInfo)
            {
                if (memberInfo.DeclaringType == typeof(SerialisableClass) && memberInfo.Name == nameof(SerialisableClass.Prop2))
                {
                    property.ShouldDeserialize = o => false;
                }
            }

            public void SetSerialisationPropertyContractDefinition(JsonProperty property, MemberInfo memberInfo)
            {
                if (memberInfo.DeclaringType == typeof(SerialisableClass) && memberInfo.Name == nameof(SerialisableClass.Prop2))
                {
                    property.ShouldSerialize = o => false;
                }
            }
        }

        #endregion

        #region JsonSerialisationContractResolver

        [Fact]
        public void ToJson_Serialisation_Default_ContractResolver()
        {
            var c = new SerialisableClass
            {
                Prop1 = "Property 1",
                Prop2 = "Property 2",
                Prop3 = "Property 3"
            };

            var result = c.ToJson();

            result.Should().NotBeEmpty();
            result.Should().Contain("Property 1");
            result.Should().Contain("Property 2");
            result.Should().Contain("Property 3");
        }

        [Fact]
        public void ToJson_Serialisation_Default_SpecificContract()
        {
            var c = new SerialisableClass
            {
                Prop1 = "Property 1",
                Prop2 = "Property 2",
                Prop3 = "Property 3"
            };

            var result = c.ToJson(new JsonSerializerSettings
            {
                ContractResolver = new JsonSerialisationContractResolver(new AntiProp2Contract())
            });

            result.Should().NotBeEmpty();
            result.Should().Contain("Property 1");
            result.Should().NotContain("Property 2");
            result.Should().Contain("Property 3");
        }

        #endregion

        #region JsonsDeserialisationContractResolver

        [Fact]
        public void ToJson_Deserialisation_Default_ContractResolver()
        {
            var json = "{" +
                "'Prop1':'Property 1'," +
                "'Prop2':'Property 2'," +
                "'Prop3':'Property 3'" +
                "}";

            var result = json.FromJson<SerialisableClass>();

            result.Should().NotBeNull();
            result.Prop1.Should().Be("Property 1");
            result.Prop2.Should().Be("Property 2");
            result.Prop3.Should().Be("Property 3");
        }

        [Fact]
        public void ToJson_Deserialisation_Default_SpecificContract()
        {
            var json = "{" +
                   "'Prop1':'Property 1'," +
                   "'Prop2':'Property 2'," +
                   "'Prop3':'Property 3'" +
                   "}";

            var result = json.FromJson<SerialisableClass>(new JsonSerializerSettings
            {
                ContractResolver = new JsonDeserialisationContractResolver(new AntiProp2Contract())
            });

            result.Should().NotBeNull();
            result.Prop1.Should().Be("Property 1");
            result.Prop2.Should().BeNullOrWhiteSpace();
            result.Prop3.Should().Be("Property 3");
        }

        #endregion

    }
}
