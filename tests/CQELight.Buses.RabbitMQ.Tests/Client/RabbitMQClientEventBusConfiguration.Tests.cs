using CQELight.Buses.RabbitMQ.Client;
using CQELight.TestFramework;
using FluentAssertions;
using System;
using Xunit;

namespace CQELight.Buses.RabbitMQ.Tests
{
    public class RabbitMQClientEventBusConfigurationTests : BaseUnitTestClass
    {
        #region Ctor & members

        #endregion

        #region Ctor 

        [Fact]
        public void RabbitMQClientEventBusConfiguration_Ctor_TestParams()
        {
            Assert.Throws<ArgumentException>(() => new RabbitMQClientBusConfiguration("", "", "", ""));
            Assert.Throws<ArgumentException>(() => new RabbitMQClientBusConfiguration("", "testserver:a:a:a:a", "", ""));
            Assert.Throws<ArgumentException>(() => new RabbitMQClientBusConfiguration("test", "testserver:a:a:a:a", "", ""));
            Assert.Throws<ArgumentException>(() => new RabbitMQClientBusConfiguration("test", "testserver:a", "", ""));

            var c = new RabbitMQClientBusConfiguration("test", "testserver:12345", "abc", "abc");
            c.Host.Should().Be("testserver");
            c.Port.Should().Be(12345);
            c.UserName.Should().Be("abc");
            c.Password.Should().Be("abc");
            c.Emiter.Should().Be("test");

            var c2 = new RabbitMQClientBusConfiguration("test", "testserver", "abc", "abc");
            c2.Host.Should().Be("testserver");
            c2.Port.Should().NotHaveValue();
            c2.UserName.Should().Be("abc");
            c2.Password.Should().Be("abc");
            c.Emiter.Should().Be("test");
        }

        #endregion

    }
}
