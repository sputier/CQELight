using CQELight.Abstractions.Configuration;
using CQELight.Abstractions.Events;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Buses.MSMQ.Client;
using CQELight.Events.Serializers;
using CQELight.TestFramework;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Buses.MSMQ.Integration.Tests
{
    public class MSMQServerTests : BaseUnitTestClass
    {

        #region Ctor & members

        private Mock<IAppIdRetriever> _serverAppId;
        private Guid _serverGuid = Guid.NewGuid();

        private Mock<IAppIdRetriever> _clientAppId;
        private Guid _clientGuid = Guid.NewGuid();
        private static bool Received = false;

        public MSMQServerTests()
        {
            _serverAppId = new Mock<IAppIdRetriever>();
            _serverAppId.Setup(m => m.GetAppId())
                .Returns(new Configuration.AppId(_serverGuid));

            _clientAppId = new Mock<IAppIdRetriever>();
            _clientAppId.Setup(m => m.GetAppId())
                .Returns(new Configuration.AppId(_clientGuid));

            Received = false;

            Tools.CleanQueue();
        }

        private class TestEvent : BaseDomainEvent
        {
            public string Data { get; set; }
        }

        #endregion

        #region Working

        [Fact]
        public async Task MSMQServer_Working_Test()
        {
            var server = new MSMQServer(_serverAppId.Object, new LoggerFactory(),
                configuration: new QueueConfiguration(new JsonDispatcherSerializer(), "", callback: o =>
                 {
                     if (o is TestEvent domainEvent)
                     {
                         Received = true;
                         domainEvent.Should().NotBeNull();
                         domainEvent.Data.Should().Be("test");
                     }
                 }));

            await server.StartAsync();

            var client = new MSMQClientBus(_clientAppId.Object, new JsonDispatcherSerializer());
            await client.RegisterAsync(new TestEvent { Data = "test" }).ConfigureAwait(false);

            uint elapsed = 0;

            while (elapsed <= 2000 && !Received) // 2sec should be enough
            {
                elapsed += 10;
                await Task.Delay(10);
            }
            Received.Should().BeTrue();
        }

        #endregion

    }
}
