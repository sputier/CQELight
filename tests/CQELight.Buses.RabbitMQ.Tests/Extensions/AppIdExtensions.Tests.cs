using CQELight.Configuration;
using CQELight.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using CQELight.Buses.RabbitMQ.Extensions;
using Xunit;
using FluentAssertions;

namespace CQELight.Buses.RabbitMQ.Tests.Extensions
{
    public class AppIdExtensionsTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region ToQueueName

        [Fact]
        public void AppIdExtensions_ToQueueName_Without_Alias()
        {
            var appId = AppId.Generate();

            var q = appId.ToQueueName();

            q.Should().NotBeNullOrWhiteSpace();
            q.Should().Be(Consts.CONST_QUEUE_NAME_PREFIX + appId.Value.ToString());
        }

        [Fact]
        public void AppIdExtensions_ToQueueName_With_Alias()
        {
            var appId = AppId.Generate("test_queue");

            var q = appId.ToQueueName();

            q.Should().NotBeNullOrWhiteSpace();
            q.Should().Be(Consts.CONST_QUEUE_NAME_PREFIX + "test_queue_" + appId.Value.ToString());
        }

        #endregion
    }
}
