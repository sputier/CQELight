using CQELight.Abstractions.Events;
using CQELight.Buses.MSMQ.Client;
using CQELight.Events.Serializers;
using CQELight.TestFramework;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace CQELight.Buses.MSMQ.Integration.Tests
{
    public class MSMQClientBusTests : BaseUnitTestClass
    {

        #region Ctor & members

        private class MSMQEvent : BaseDomainEvent
        {
            public string Data { get; set; }
        }

        private const string CONST_APP_ID = "AA3F9093-D7EE-4BB8-9B4E-EEC3447A89BA";


        public MSMQClientBusTests()
        {

        }

        #endregion

        #region RegisterEvent

        [Fact]
        public async Task MSMQClientBus_RegisterAsync_AsExpected()
        {
            Tools.CleanQueue();
            var evt = new MSMQEvent
            {
                Data = "testData"
            };

            var b = new MSMQClientBus(
                CONST_APP_ID,
                new JsonDispatcherSerializer(),
                new MSMQClientBusConfiguration());

            await b.PublishEventAsync(evt).ConfigureAwait(false);

            var q = Helpers.GetMessageQueue();

            var messages = q.GetAllMessages();
            messages.Should().NotBeEmpty();

        }

        #endregion

    }
}
