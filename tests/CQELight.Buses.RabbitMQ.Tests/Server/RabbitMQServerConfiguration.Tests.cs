using CQELight.Buses.RabbitMQ.Server;
using CQELight.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CQELight.Buses.RabbitMQ.Tests.Server
{
    public class RabbitMQServerConfigurationTests : BaseUnitTestClass
    {

        #region Ctor & members

        #endregion

        #region Ctor

        [Fact]
        public void RabbitMQServerConfiguration_Ctor_Params()
        {
            Assert.Throws<ArgumentException>(() => new RabbitMQServerConfiguration("host", "user", "pawd", new QueueConfiguration[0]));
            Assert.Throws<ArgumentException>(() => new RabbitMQServerConfiguration("host", "user", "pawd", null));
        }

        #endregion

    }
}
