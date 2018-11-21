using CQELight.TestFramework;
using CQELight.Tools.Extensions;
using CQELight.Tools.Serialisation;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

namespace CQELight.Tools.Tests.Extensions
{
    public class StringExtensionsTests : BaseUnitTestClass
    {
        #region FromJson

        [Fact]
        public void FromJson_NullString()
        {
            string.Empty.FromJson<object>().Should().BeNull();
        }

        [Fact]
        public void FromJson_AsExpected()
        {
            DateTime d = new DateTime(2017, 01, 01);
            var json = JsonConvert.SerializeObject(d);

            var dTest = json.FromJson<DateTime>();

            dTest.Should().BeSameDateAs(d);
        }
        private class JsonSerialized
        {
            public string PublicData { get; set; }
            private string _privateData;
            
            public bool PrivateDataIsEmpty => string.IsNullOrWhiteSpace(_privateData);
        }

        [Fact]
        public void FromJson_SpecificContracts_Should_Be_Used()
        {
            var contract = new Mock<IJsonContractDefinition>();

            contract.Setup(m => m.SetDeserialisationPropertyContractDefinition(It.IsAny<JsonProperty>(), It.IsAny<MemberInfo>()))
                .Callback((JsonProperty prop, MemberInfo m) =>
                {
                    if (m.Name == "PublicData")
                    {
                        prop.ShouldDeserialize = _ => false;
                    }
                });

            var json = "{'PublicData':'public', '_privateData':'private'}";

            var obj = json.FromJson<JsonSerialized>(contract.Object);

            obj.PublicData.Should().BeNullOrEmpty();
        }

        #endregion

        #region WriteToStream

        [Fact]
        public void WriteToStream_NullStream()
        {
            var buffer = new byte[100];
            Assert.Throws<ArgumentNullException>(() => "foo".WriteToStream(null));
            Assert.Throws<ArgumentNullException>(() => "foo".WriteToStream(new MemoryStream(buffer, false)));
        }

        [Fact]
        public void WriteToStream_EmptyString()
        {
            string.Empty.WriteToStream(new MemoryStream(new byte[100])).Should().Be(0);
        }

        [Fact]
        public void WriteToStream_AsExpected()
        {
            var buffer = new byte[100];
            var stream = new MemoryStream(buffer);
            "foo".WriteToStream(stream).Should().Be(3);
            stream.Position = 0;
            using (var streamR = new StreamReader(stream))
            {
                streamR.ReadLine().Trim('\0').Should().Be("foo");
            }
        }

        #endregion

    }
}
