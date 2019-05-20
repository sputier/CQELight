using CQELight.Abstractions.Events;
using CQELight.Buses.MSMQ.Client;
using CQELight.Events.Serializers;
using CQELight.TestFramework;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Buses.MSMQ.Integration.Tests
{
    public class MSMQServerTests : BaseUnitTestClass
    {

        #region Ctor & members


        private class TestEvent : BaseDomainEvent
        {
            public string Data { get; set; }
        }

        #endregion

        #region Working

        [Fact]
        public async Task MSMQServer_Working_Test()
        {
            Tools.CleanQueue();
            bool received = false;
            var server = new MSMQServer("DA8C3F43-36C5-45F8-A773-1F11C0B77223", new LoggerFactory(),
                configuration: new QueueConfiguration(new JsonDispatcherSerializer(), "", callback: o =>
                 {
                     if (o is TestEvent domainEvent)
                     {
                         received = domainEvent != null && domainEvent.Data == "test";
                     }
                 }));

            await server.StartAsync();

            var client = new MSMQClientBus("DA8C3F43-36C5-45F8-A773-1F11C0B77224", new JsonDispatcherSerializer());
            await client.PublishEventAsync(new TestEvent { Data = "test" }).ConfigureAwait(false);

            uint elapsed = 0;

            while (elapsed <= 2000 && !received) // 2sec should be enough
            {
                elapsed += 10;
                await Task.Delay(10);
            }
            received.Should().BeTrue();
        }

        #endregion

    }
}
