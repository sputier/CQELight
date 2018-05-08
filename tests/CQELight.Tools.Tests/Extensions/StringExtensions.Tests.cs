using CQELight.TestFramework;
using CQELight.Tools.Extensions;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace CQELight.Tools.Tests.Extensions
{
    public class StringExtensionsTests : BaseUnitTestClass
    {
        #region FromJson

        [Fact]
        public void StringExtensions_FromJson_NullString()
        {
            string.Empty.FromJson<object>().Should().BeNull();
        }

        [Fact]
        public void StringExtensions_FromJson_AsExpected()
        {
            DateTime d = new DateTime(2017, 01, 01);
            var json = JsonConvert.SerializeObject(d);

            var dTest = json.FromJson<DateTime>();

            dTest.Should().BeSameDateAs(d);
        }

        #endregion

        #region WriteToStream

        [Fact]
        public void StringExtensions_WriteToStream_NullStream()
        {
            var buffer = new byte[100];
            Assert.Throws<ArgumentNullException>(() => "foo".WriteToStream(null));
            Assert.Throws<ArgumentNullException>(() => "foo".WriteToStream(new MemoryStream(buffer, false)));
        }

        [Fact]
        public void StringExtensions_WriteToStream_EmptyString()
        {
            string.Empty.WriteToStream(new MemoryStream(new byte[100])).Should().Be(0);
        }

        [Fact]
        public void StringExtensions_WriteToStream_AsExpected()
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
